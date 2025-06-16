using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using MigrationTools.Tools;
using MigrationTools.Tools.Infrastructure;
using NCalc;

namespace MigrationTools.FieldMaps.AzureDevops.ObjectModel
{
    /// <summary>
    /// Performs mathematical calculations on numeric fields using NCalc expressions during migration.
    /// </summary>
    public class FieldCalculationMap : FieldMapBase
    {
        /// <summary>
        /// Initializes a new instance of the FieldCalculationMap class.
        /// </summary>
        /// <param name="logger">Logger for the field map operations</param>
        /// <param name="telemetryLogger">Telemetry logger for tracking operations</param>
        public FieldCalculationMap(ILogger<FieldCalculationMap> logger, ITelemetryLogger telemetryLogger) 
            : base(logger, telemetryLogger)
        {
        }

        private FieldCalculationMapOptions Config { get { return (FieldCalculationMapOptions)_Config; } }

        /// <summary>
        /// Configures the field map with the specified options and validates required settings.
        /// </summary>
        /// <param name="config">The field map configuration options</param>
        /// <exception cref="ArgumentNullException">Thrown when required fields are not specified</exception>
        public override void Configure(IFieldMapOptions config)
        {
            base.Configure(config);

            if (string.IsNullOrEmpty(Config.expression))
            {
                throw new ArgumentNullException(nameof(Config.expression), "The expression field must be specified.");
            }

            if (string.IsNullOrEmpty(Config.targetField))
            {
                throw new ArgumentNullException(nameof(Config.targetField), "The target field must be specified.");
            }

            if (Config.parameters == null || Config.parameters.Count == 0)
            {
                throw new ArgumentNullException(nameof(Config.parameters), "At least one parameter mapping must be specified.");
            }
        }

        public override string MappingDisplayName => $"{Config.expression} -> {Config.targetField}";

        internal override void InternalExecute(WorkItem source, WorkItem target)
        {
            try
            {
                // Validate that target field exists
                if (!target.Fields.Contains(Config.targetField))
                {
                    Log.LogWarning("FieldCalculationMap: Target field '{TargetField}' does not exist on work item {WorkItemId}. Skipping calculation.", Config.targetField, target.Id);
                    return;
                }

                // Validate that all source fields exist and collect their values
                var parameterValues = new Dictionary<string, object>();
                foreach (var parameter in Config.parameters)
                {
                    if (!source.Fields.Contains(parameter.Value))
                    {
                        Log.LogWarning("FieldCalculationMap: Source field '{SourceField}' does not exist on work item {WorkItemId}. Skipping calculation.", parameter.Value, source.Id);
                        return;
                    }

                    var fieldValue = source.Fields[parameter.Value].Value;
                    if (fieldValue == null)
                    {
                        Log.LogWarning("FieldCalculationMap: Source field '{SourceField}' is null on work item {WorkItemId}. Skipping calculation.", parameter.Value, source.Id);
                        return;
                    }

                    // Convert field value to numeric
                    if (!TryConvertToNumeric(fieldValue, out var numericValue))
                    {
                        Log.LogWarning("FieldCalculationMap: Source field '{SourceField}' with value '{FieldValue}' is not numeric on work item {WorkItemId}. Skipping calculation.", parameter.Value, fieldValue, source.Id);
                        return;
                    }

                    parameterValues[parameter.Key] = numericValue;
                }

                // Evaluate the expression
                var expression = new Expression(Config.expression);
                
                // Set parameters
                foreach (var param in parameterValues)
                {
                    expression.Parameters[param.Key] = param.Value;
                }

                // Evaluate and get result
                var result = expression.Evaluate();
                
                if (expression.HasErrors())
                {
                    Log.LogError("FieldCalculationMap: Expression evaluation failed with error: {Error} for work item {WorkItemId}", expression.Error, source.Id);
                    return;
                }

                // Convert result to appropriate numeric type and set target field
                if (TryConvertToTargetFieldType(result, target.Fields[Config.targetField], out var convertedResult))
                {
                    target.Fields[Config.targetField].Value = convertedResult;
                    Log.LogDebug("FieldCalculationMap: Successfully calculated and set field '{TargetField}' to '{Result}' for work item {WorkItemId}", Config.targetField, convertedResult, target.Id);
                }
                else
                {
                    Log.LogWarning("FieldCalculationMap: Could not convert calculation result '{Result}' to target field type for work item {WorkItemId}", result, target.Id);
                }
            }
            catch (Exception ex)
            {
                Log.LogError(ex, "FieldCalculationMap: Unexpected error during calculation for work item {WorkItemId}", target.Id);
            }
        }

        /// <summary>
        /// Attempts to convert a field value to a numeric type.
        /// </summary>
        /// <param name="value">The field value to convert</param>
        /// <param name="numericValue">The converted numeric value</param>
        /// <returns>True if conversion was successful, false otherwise</returns>
        private static bool TryConvertToNumeric(object value, out object numericValue)
        {
            numericValue = null;

            if (value is int || value is long || value is decimal || value is double || value is float)
            {
                numericValue = value;
                return true;
            }

            var stringValue = value.ToString().Trim();
            
            // Try different numeric types
            if (int.TryParse(stringValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var intValue))
            {
                numericValue = intValue;
                return true;
            }

            if (long.TryParse(stringValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var longValue))
            {
                numericValue = longValue;
                return true;
            }

            if (double.TryParse(stringValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var doubleValue))
            {
                numericValue = doubleValue;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Attempts to convert the calculation result to the appropriate type for the target field.
        /// </summary>
        /// <param name="result">The calculation result</param>
        /// <param name="targetField">The target field</param>
        /// <param name="convertedResult">The converted result</param>
        /// <returns>True if conversion was successful, false otherwise</returns>
        private static bool TryConvertToTargetFieldType(object result, Field targetField, out object convertedResult)
        {
            convertedResult = null;

            try
            {
                // Check target field type and convert accordingly
                var fieldType = targetField.FieldDefinition.FieldType;
                
                switch (fieldType)
                {
                    case FieldType.Integer:
                        if (result is double doubleResult)
                        {
                            convertedResult = (int)Math.Round(doubleResult);
                        }
                        else
                        {
                            convertedResult = Convert.ToInt32(result);
                        }
                        return true;

                    case FieldType.Double:
                        convertedResult = Convert.ToDouble(result);
                        return true;

                    case FieldType.String:
                        // Allow setting string fields with numeric results
                        convertedResult = result.ToString();
                        return true;

                    default:
                        // For other field types, try direct assignment
                        convertedResult = result;
                        return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}