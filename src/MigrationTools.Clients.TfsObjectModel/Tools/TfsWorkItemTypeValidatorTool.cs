using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using MigrationTools.FieldMaps.AzureDevops.ObjectModel;
using MigrationTools.Tools.Infrastructure;
using MigrationTools.Tools.Interfaces;

namespace MigrationTools.Tools
{
    /// <summary>
    /// This tool checks if the work item types in the source system have corresponding types in the target system,
    /// and validates their fields and mappings.
    /// </summary>
    public class TfsWorkItemTypeValidatorTool : Tool<TfsWorkItemTypeValidatorToolOptions>
    {
        private class WitMapping
        {
            public WitMapping(WorkItemType sourceWit, string expectedTargetWitName, WorkItemType? targetWit, bool isMapped)
            {
                SourceWit = sourceWit;
                ExpectedTargetWitName = expectedTargetWitName;
                TargetWit = targetWit;
                IsMapped = isMapped;
            }

            public WorkItemType SourceWit { get; }
            public string ExpectedTargetWitName { get; }
            public WorkItemType? TargetWit { get; }
            public bool IsMapped { get; }
        }

        private readonly IWorkItemTypeMappingTool _witMappingTool;
        private readonly CommonTools _commonTools;

        public TfsWorkItemTypeValidatorTool(
            IOptions<TfsWorkItemTypeValidatorToolOptions> options,
            IWorkItemTypeMappingTool witMappingTool,
            CommonTools commonTools,
            IServiceProvider services,
            ILogger<TfsWorkItemTypeValidatorTool> logger,
            ITelemetryLogger telemetry)
            : base(options, services, logger, telemetry)
        {
            _commonTools = commonTools ?? throw new ArgumentNullException(nameof(commonTools));
            _witMappingTool = witMappingTool ?? throw new ArgumentNullException(nameof(witMappingTool));
        }

        public bool ValidateReflectedWorkItemIdField(
            List<WorkItemType> sourceWits,
            List<WorkItemType> targetWits,
            string reflectedWorkItemIdField)
        {
            Log.LogInformation("Validating presence of reflected work item ID field '{reflectedWorkItemIdField}'"
                + " in target work item types.", reflectedWorkItemIdField);
            bool isValid = true;
            List<WitMapping> witPairs = GetWitsToValidate(sourceWits, targetWits);
            foreach (WitMapping witPair in witPairs)
            {
                WorkItemType? targetWit = witPair.TargetWit;
                if (targetWit is null)
                {
                    continue;
                }
                if (targetWit.FieldDefinitions.Contains(reflectedWorkItemIdField))
                {
                    Log.LogDebug("'{targetWit}' contains reflected work item ID field '{fieldName}'.",
                        targetWit.Name, reflectedWorkItemIdField);
                }
                else
                {
                    Log.LogError("'{targetWit}' does not contain reflected work item ID field '{fieldName}'.",
                        targetWit.Name, reflectedWorkItemIdField);
                    isValid = false;
                }
            }
            LogReflectedWorkItemIdValidationResult(isValid, reflectedWorkItemIdField);
            return isValid;
        }

        // Returns list of target work item types with respect to work item type mapping.
        private List<WitMapping> GetWitsToValidate(List<WorkItemType> sourceWits, List<WorkItemType> targetWits)
        {
            List<WitMapping> witMappings = [];
            foreach (WorkItemType sourceWit in sourceWits)
            {
                if (ShouldValidateWorkItemType(sourceWit.Name))
                {
                    bool isMapped = false;
                    if (_witMappingTool.Mappings.TryGetValue(sourceWit.Name, out string targetWitName))
                    {
                        isMapped = true;
                    }
                    else
                    {
                        targetWitName = sourceWit.Name;
                    }
                    WorkItemType? targetWit = targetWits
                        .FirstOrDefault(wit => wit.Name.Equals(targetWitName, StringComparison.OrdinalIgnoreCase));
                    witMappings.Add(new WitMapping(sourceWit, targetWitName, targetWit, isMapped));
                }
            }
            return witMappings;
        }

        public bool ValidateWorkItemTypes(List<WorkItemType> sourceWits, List<WorkItemType> targetWits)
        {
            LogWorkItemTypes(sourceWits, targetWits);

            List<string> targetWitNames = targetWits.Select(wit => wit.Name).ToList();
            bool isValid = true;
            List<WitMapping> witPairs = GetWitsToValidate(sourceWits, targetWits);
            foreach (WitMapping witPair in witPairs)
            {
                string sourceWitName = witPair.SourceWit.Name;
                Log.LogInformation("Validating work item type '{sourceWit}'.", sourceWitName);
                if (witPair.IsMapped)
                {
                    Log.LogInformation("  Work item type '{sourceWit}' is mapped to '{targetWit}'.",
                        sourceWitName, witPair.ExpectedTargetWitName);
                }
                if (witPair.TargetWit is null)
                {
                    Log.LogWarning("Work item type '{targetWit}' does not exist in target system.", witPair.ExpectedTargetWitName);
                    if (TryFindSimilarWorkItemType(sourceWitName, targetWitNames, out string suggestedName))
                    {
                        Log.LogInformation(" Suggested mapping: '{0}' – '{1}'", sourceWitName, suggestedName);
                    }
                    isValid = false;
                }
                else
                {
                    if (!ValidateWorkItemTypeFields(witPair.SourceWit, witPair.TargetWit))
                    {
                        isValid = false;
                    }
                }
            }
            LogValidationResult(isValid);
            return isValid;
        }

        private bool ValidateWorkItemTypeFields(WorkItemType sourceWit, WorkItemType targetWit)
        {
            bool result = true;
            List<FieldDefinition> sourceFields = sourceWit.FieldDefinitions.Cast<FieldDefinition>().ToList();
            Dictionary<string, FieldDefinition> targetFields = targetWit.FieldDefinitions.Cast<FieldDefinition>()
                .ToDictionary(f => f.ReferenceName, f => f, StringComparer.OrdinalIgnoreCase);
            foreach (FieldDefinition sourceField in sourceFields)
            {
                string sourceFieldName = sourceField.ReferenceName;
                string targetFieldName = GetTargetFieldName(targetWit.Name, sourceFieldName);
                if (string.IsNullOrEmpty(targetFieldName))
                {
                    continue;
                }

                if (targetFields.ContainsKey(targetFieldName))
                {
                    if (sourceField.IsIdentity)
                    {
                        const string message = "  Source field '{sourceFieldName}' is identity field."
                            + " Validation is not performed on identity fields, because they usually differ in allowed values.";
                        Log.LogDebug(message, sourceFieldName);
                    }
                    else
                    {
                        result &= ValidateField(sourceField, targetFields[targetFieldName], targetWit.Name);
                    }
                }
                else
                {
                    Log.LogWarning("  Missing field '{targetFieldName}' in '{targetWit}'.", targetFieldName, targetWit.Name);
                    Log.LogInformation("    Source field reference name: {sourceFieldReferenceName}", sourceFieldName);
                    Log.LogInformation("    Source field name: {sourceFieldName}", sourceField.Name);
                    Log.LogInformation("    Field type: {fieldType}", sourceField.FieldType);
                    (string valueType, List<string> allowedValues) = GetAllowedValues(sourceField);
                    Log.LogInformation("    Allowed values: {allowedValues}", string.Join(", ", allowedValues.Select(v => $"'{v}'")));
                    Log.LogInformation("    Allowed values type: {allowedValuesType}", valueType);
                    result = false;
                }
            }

            if (result)
            {
                Log.LogInformation("  All fields are either present or mapped.");
            }
            return result;
        }

        private bool ValidateField(FieldDefinition sourceField, FieldDefinition targetField, string targetWitName)
        {
            // If target field is in 'FixedTargetFields' list, it means, that user resolved this filed somehow.
            // For example by value mapping. So any discrepancies found will be logged just as information.
            // Otherwise, discrepancies are logged as warning.
            LogLevel logLevel = Options.IsFieldFixed(targetWitName, targetField.ReferenceName)
                ? LogLevel.Information
                : LogLevel.Warning;
            bool isValid = ValidateFieldType(sourceField, targetField, logLevel);
            isValid &= ValidateFieldAllowedValues(targetWitName, sourceField, targetField, logLevel);
            if (isValid)
            {
                Log.LogDebug("  Target field '{targetFieldName}' exists in '{targetWit}' and is valid.",
                    targetField.ReferenceName, targetWitName);
            }
            else if (logLevel == LogLevel.Information)
            {
                Log.LogInformation("  Target field '{targetFieldName}' in '{targetWit}' is considered valid,"
                    + $" because it is listed in '{nameof(Options.FixedTargetFields)}'.",
                    targetField.ReferenceName, targetWitName, sourceField.ReferenceName);
            }
            return (logLevel == LogLevel.Information) || isValid;
        }

        private bool ValidateFieldType(FieldDefinition sourceField, FieldDefinition targetField, LogLevel logLevel)
        {
            if (sourceField.FieldType != targetField.FieldType)
            {
                Log.Log(logLevel,
                    "  Source field '{sourceField}' and target field '{targetField}' have different types:"
                    + " source = '{sourceFieldType}', target = '{targetFieldType}'.",
                    sourceField.ReferenceName, targetField.ReferenceName, sourceField.FieldType, targetField.FieldType);
                return false;
            }
            return true;
        }

        private bool ValidateFieldAllowedValues(
            string targetWitName,
            FieldDefinition sourceField,
            FieldDefinition targetField,
            LogLevel logLevel)
        {
            bool isValid = true;
            (string sourceValueType, List<string> sourceAllowedValues) = GetAllowedValues(sourceField);
            (string targetValueType, List<string> targetAllowedValues) = GetAllowedValues(targetField);
            if (!sourceValueType.Equals(targetValueType, StringComparison.OrdinalIgnoreCase))
            {
                isValid = false;
                Log.Log(logLevel,
                    "  Source field '{sourceField}' and target field '{targetField}' have different allowed values types:"
                    + " source = '{sourceFieldAllowedValueType}', target = '{targetFieldAllowedValueType}'.",
                    sourceField.ReferenceName, targetField.ReferenceName, sourceValueType, targetValueType);
            }
            if (!ValidateAllowedValues(targetWitName, sourceField.ReferenceName, sourceAllowedValues,
                targetField.ReferenceName, targetAllowedValues, out List<string> missingValues))
            {
                isValid = false;
                Log.Log(logLevel,
                    "  Source field '{sourceField}' and target field '{targetField}' have different allowed values.",
                    sourceField.ReferenceName, targetField.ReferenceName);
                LogAllowedValues("    Source allowed values: {sourceAllowedValues}", sourceAllowedValues);
                LogAllowedValues("    Target allowed values: {targetAllowedValues}", targetAllowedValues);
                LogAllowedValues("    Missing values in target are: {missingValues}", missingValues);
                Log.LogInformation($"    You can configure value mapping using '{nameof(FieldValueMap)}' in '{nameof(FieldMappingTool)}',"
                    + " or change the process of target system to contain all missing allowed values.");
            }

            return isValid;

            void LogAllowedValues(string message, List<string> values)
                => Log.LogInformation(message, string.Join(", ", values.Select(value => $"'{value}'")));
        }

        private bool ValidateAllowedValues(
            string targetWitName,
            string sourceFieldReferenceName,
            List<string> sourceAllowedValues,
            string targetFieldReferenceName,
            List<string> targetAllowedValues,
            out List<string> missingValues)
        {
            missingValues = sourceAllowedValues
                .Except(targetAllowedValues, StringComparer.OrdinalIgnoreCase)
                .ToList();
            if (missingValues.Count > 0)
            {
                Log.LogDebug("  Allowed values in target do not match allowed values in source. Checking field value maps.");
                Log.LogInformation("  Missing values are: {missingValues}", string.Join(", ", missingValues.Select(val => $"'{val}'")));
                List<string> mappedValues = [];
                Dictionary<string, string> valueMaps = _commonTools.FieldMappingTool
                    .GetFieldValueMappings(targetWitName, sourceFieldReferenceName, targetFieldReferenceName);
                foreach (string missingValue in missingValues)
                {
                    if (valueMaps.TryGetValue(missingValue, out string mappedValue))
                    {
                        if (targetAllowedValues.Contains(mappedValue, StringComparer.OrdinalIgnoreCase))
                        {
                            mappedValues.Add(missingValue);
                            Log.LogDebug("    Value '{missingValue}' is mapped to '{mappedValue}', which exists in target.",
                                missingValue, mappedValue);
                        }
                        else
                        {
                            Log.LogWarning("    Value '{missingValue}' is mapped to '{mappedValue}', which does not exists in target."
                                + $" This is probably invalid '{nameof(FieldValueMap)}' configuration.",
                                missingValue, mappedValue);
                        }
                    }
                }
                missingValues = missingValues
                    .Except(mappedValues, StringComparer.OrdinalIgnoreCase)
                    .ToList();
            }
            return missingValues.Count == 0;
        }


        private (string valueType, List<string> allowedValues) GetAllowedValues(FieldDefinition field)
        {
            string valueType = field.SystemType.Name;
            List<string> allowedValues = [];
            for (int i = 0; i < field.AllowedValues.Count; i++)
            {
                allowedValues.Add(field.AllowedValues[i]);
            }
            return (valueType, allowedValues);
        }

        private bool ShouldValidateWorkItemType(string workItemTypeName)
        {
            if ((Options.IncludeWorkItemtypes.Count > 0)
                && !Options.IncludeWorkItemtypes.Contains(workItemTypeName, StringComparer.OrdinalIgnoreCase))
            {
                Log.LogInformation(
                    "Skipping validation of work item type '{sourceWit}' because it is not included in validation list.",
                    workItemTypeName);
                return false;
            }
            else if ((Options.ExcludeWorkItemtypes.Count > 0)
                && Options.ExcludeWorkItemtypes.Contains(workItemTypeName, StringComparer.OrdinalIgnoreCase))
            {
                Log.LogInformation(
                    "Skipping validation of work item type '{sourceWit}' because it is excluded from validation list.",
                    workItemTypeName);
                return false;
            }
            return true;
        }

        private string GetTargetWorkItemType(string sourceWit)
        {
            string targetWit = sourceWit;
            if (_witMappingTool.Mappings.ContainsKey(sourceWit))
            {
                targetWit = _witMappingTool.Mappings[sourceWit];
                Log.LogInformation("  This work item type is mapped to '{targetWit}' in target.", targetWit);
            }
            return targetWit;
        }

        private void LogWorkItemTypes(ICollection<WorkItemType> sourceWits, ICollection<WorkItemType> targetWits)
        {
            Log.LogInformation(
                "Validating work item types.");
            Log.LogInformation("Source work item types are: {sourceWits}.",
                string.Join(", ", sourceWits.Select(wit => wit.Name)));
            Log.LogInformation("Target work item types are: {targetWits}.",
                string.Join(", ", targetWits.Select(wit => wit.Name)));
        }

        private void LogReflectedWorkItemIdValidationResult(bool isValid, string reflectedWorkItemIdField)
        {
            if (isValid)
            {
                Log.LogInformation("All work item types have reflected work item ID field '{reflectedWorkItemIdField}'.",
                    reflectedWorkItemIdField);
                return;
            }

            const string message = "Reflected work item ID field is mandatory for work item migration."
                + " You can configure name of this field in target TFS endpoint settings as 'ReflectedWorkItemIdField' property."
                + " Your current configured name of the field is '{reflectedWorkItemIdField}'.";
            Log.LogError(message, reflectedWorkItemIdField);
        }

        private void LogValidationResult(bool isValid)
        {
            if (isValid)
            {
                Log.LogInformation("All work item types are valid.");
                return;
            }

            const string message1 = "Some work item types or their fields are not present in the target system (see previous logs)."
                + " Either add these fields into target work items, or map source fields to other target fields"
                + $" in options ({nameof(TfsWorkItemTypeValidatorToolOptions.SourceFieldMappings)}).";
            Log.LogError(message1);
            const string message2 = "If you have some field mappings defined for validation, do not forget also to configure"
                + $" proper field mapping in {nameof(FieldMappingTool)} so data will preserved during migration.";
            Log.LogInformation(message2);
            const string message3 = "If you have different allowed values in some field, either update target field to match"
                + $" allowed values from source, or configure {nameof(FieldValueMap)} in {nameof(FieldMappingTool)}.";
            Log.LogInformation(message3);
        }

        private static bool TryFindSimilarWorkItemType(
            string sourceWitName,
            List<string> targetWitNames,
            out string? suggestedName)
        {
            foreach (string targetWit in targetWitNames)
            {
                if (AreVisuallySimilar(sourceWitName, targetWit))
                {
                    suggestedName = targetWit;
                }
            }
            suggestedName = null;
            return false;
        }

        private static bool AreVisuallySimilar(string a, string b)
        {
            if (a.Equals(b, StringComparison.OrdinalIgnoreCase))
            {
                // Already matched normally
                return false;
            }

            if (a.Length != b.Length)
            {
                return false;
            }

            int similarCount = 0;

            for (int i = 0; i < a.Length; i++)
            {
                var aChar = a[i];
                var bChar = b[i];

                if (aChar == bChar)
                {
                    similarCount++;
                    continue;
                }

                // Check known lookalike characters (expandable)
                if ((aChar == '\u0399' && bChar == 'I') || (aChar == 'I' && bChar == '\u0399'))
                {
                    similarCount++;
                }
                else
                {
                    return false;
                }
            }

            return similarCount == a.Length;
        }
    }
}
