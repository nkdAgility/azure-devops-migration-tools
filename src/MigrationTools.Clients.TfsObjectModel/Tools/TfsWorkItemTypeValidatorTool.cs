using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
            public WitMapping(
                WorkItemType sourceWit,
                string expectedTargetWitName,
                WorkItemType? targetWit,
                bool isMapped)
            {
                SourceWit = sourceWit;
                ExpectedTargetWitName = expectedTargetWitName;
                TargetWit = targetWit;
                IsMapped = isMapped;
            }

            public WorkItemType SourceWit { get; }
            public string ExpectedTargetWitName { get; }
            public WorkItemType? TargetWit { get; }
            public bool HasTargetWit => TargetWit is not null;
            public bool IsMapped { get; }
        }

        private class FieldMapping
        {
            public FieldMapping(
                FieldDefinition sourceField,
                string expectedTargetFieldName,
                FieldDefinition? targetField,
                bool isMapped)
            {
                SourceField = sourceField;
                ExpectedTargetFieldName = expectedTargetFieldName;
                TargetField = targetField;
                IsMapped = isMapped;
            }

            public FieldDefinition SourceField { get; }
            public string ExpectedTargetFieldName { get; }
            public FieldDefinition? TargetField { get; }
            public bool HasTargetField => TargetField is not null;
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
                if (!witPair.HasTargetWit)
                {
                    continue;
                }
                WorkItemType targetWit = witPair.TargetWit;
                if (witPair.IsMapped)
                {
                    Log.LogInformation("Work item type '{sourceWit}' is mapped to '{targetWit}'.",
                        witPair.SourceWit.Name, witPair.ExpectedTargetWitName);
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
                Log.LogInformation("Start validation of work item type '{sourceWit}'.", sourceWitName);
                if (witPair.IsMapped)
                {
                    Log.LogInformation("  Work item type '{sourceWit}' is mapped to '{targetWit}'.",
                        sourceWitName, witPair.ExpectedTargetWitName);
                }
                if (!witPair.HasTargetWit)
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
                Log.LogInformation("End validation of work item type '{sourceWit}'.", sourceWitName);
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
                List<FieldMapping> fieldsToValidate = GetTargetFieldsToValidate(targetWit.Name, sourceField, targetFields);
                foreach (FieldMapping fieldPair in fieldsToValidate)
                {
                    Log.LogInformation("  Validating source field '{sourceFieldReferenceName}' ({sourceFieldName}).",
                        sourceField.ReferenceName, sourceField.Name);
                    bool fieldResult = true;
                    bool fieldIsExcluded = Options.IsSourceFieldExcluded(sourceWit.Name, sourceField.ReferenceName);
                    LogLevel logLevel = fieldIsExcluded ? LogLevel.Information : LogLevel.Warning;
                    if (fieldPair.IsMapped)
                    {
                        Log.LogInformation("    Source field '{sourceFieldName}' is mapped to '{targetFieldName}'.",
                            fieldPair.SourceField.ReferenceName, fieldPair.ExpectedTargetFieldName);
                    }
                    if (fieldPair.HasTargetField)
                    {
                        if (sourceField.IsIdentity)
                        {
                            const string message = "    Source field '{sourceFieldName}' is identity field."
                                + " Validation is not performed on identity fields, because they usually differ in allowed values.";
                            Log.LogDebug(message, fieldPair.SourceField.ReferenceName);
                        }
                        else
                        {
                            fieldResult = ValidateField(sourceField, fieldPair.TargetField, targetWit.Name, logLevel);
                        }
                    }
                    else
                    {
                        LogMissingField(targetWit, sourceField, fieldPair.ExpectedTargetFieldName, logLevel);
                        fieldResult = false;
                    }

                    if (!fieldResult)
                    {
                        if (fieldIsExcluded)
                        {
                            Log.LogInformation("    Field '{sourceFieldName}' is excluded from validation, so it is considered valid.",
                                fieldPair.SourceField.ReferenceName);
                        }
                        else
                        {
                            result = false;
                        }
                    }
                }
            }

            if (result)
            {
                Log.LogInformation("  All fields are either present or mapped for '{targetWit}'.", targetWit.Name);
            }
            return result;
        }

        private List<FieldMapping> GetTargetFieldsToValidate(
            string targetWitName,
            FieldDefinition sourceField,
            Dictionary<string, FieldDefinition> targetFields)
        {
            List<FieldMapping> result = [];

            foreach (FieldToFieldMap fieldToFieldMap in _commonTools.FieldMappingTool
                .GetFieldToFieldMaps(targetWitName, sourceField.ReferenceName, FieldMapMode.SourceToTarget))
            {
                targetFields.TryGetValue(fieldToFieldMap.Config.targetField, out FieldDefinition targetField);
                result.Add(new FieldMapping(sourceField, fieldToFieldMap.Config.targetField, targetField, true));
            }

            foreach ((string _, string targetFieldName) in _commonTools.FieldMappingTool
                .GetFieldToFieldMultiMaps(targetWitName, sourceField.ReferenceName))
            {
                targetFields.TryGetValue(targetFieldName, out FieldDefinition targetField);
                result.Add(new FieldMapping(sourceField, targetFieldName, targetField, true));
            }

            if (result.Count == 0)
            {
                // If no field mapping is configured for this source field, just use the same field in the target.
                targetFields.TryGetValue(sourceField.ReferenceName, out FieldDefinition targetField);
                result.Add(new FieldMapping(sourceField, sourceField.ReferenceName, targetField, false));
            }

            return result;
        }

        private void LogMissingField(
            WorkItemType targetWit,
            FieldDefinition sourceField,
            string targetFieldName,
            LogLevel logLevel)
        {
            Log.Log(logLevel, "    Missing field '{targetFieldName}' in '{targetWit}'.", targetFieldName, targetWit.Name);
            Log.LogInformation("      Source field reference name: {sourceFieldReferenceName}", sourceField.ReferenceName);
            Log.LogInformation("      Source field name: {sourceFieldName}", sourceField.Name);
            Log.LogInformation("      Field type: {fieldType}", sourceField.FieldType);
            (string valueType, List<string> allowedValues) = GetAllowedValues(sourceField);
            LogAllowedValues("      Allowed values: {allowedValues}", allowedValues);
            Log.LogInformation("      Allowed values type: {allowedValuesType}", valueType);
        }

        private bool ValidateField(
            FieldDefinition sourceField,
            FieldDefinition targetField,
            string targetWitName,
            LogLevel validationLogLevel)
        {
            bool isValid = ValidateFieldType(sourceField, targetField, validationLogLevel);
            isValid &= ValidateFieldAllowedValues(targetWitName, sourceField, targetField, validationLogLevel);
            if (isValid)
            {
                Log.LogDebug("    Target field '{targetFieldReferenceName}' ({targetFieldName}) exists in '{targetWit}' and is valid.",
                    targetField.ReferenceName, targetField.Name, targetWitName);
            }
            return isValid;
        }

        private bool ValidateFieldType(FieldDefinition sourceField, FieldDefinition targetField, LogLevel logLevel)
        {
            if (sourceField.FieldType != targetField.FieldType)
            {
                Log.Log(logLevel,
                    "    Source field '{sourceField}' and target field '{targetField}' have different types:"
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
                    "    Source field '{sourceField}' and target field '{targetField}' have different allowed values types:"
                    + " source = '{sourceFieldAllowedValueType}', target = '{targetFieldAllowedValueType}'.",
                    sourceField.ReferenceName, targetField.ReferenceName, sourceValueType, targetValueType);
            }
            if (!ValidateAllowedValues(targetWitName, sourceField.ReferenceName, sourceAllowedValues,
                targetField.ReferenceName, targetAllowedValues, out List<string> missingValues, logLevel))
            {
                isValid = false;
                Log.Log(logLevel,
                    "    Source field '{sourceField}' and target field '{targetField}' have different allowed values.",
                    sourceField.ReferenceName, targetField.ReferenceName);
                LogAllowedValues("      Source allowed values: {sourceAllowedValues}", sourceAllowedValues);
                LogAllowedValues("      Target allowed values: {targetAllowedValues}", targetAllowedValues);
                LogAllowedValues("      Missing values in target are: {missingValues}", missingValues);
            }

            return isValid;

        }

        private void LogAllowedValues(string message, List<string> values)
            => Log.LogInformation(message, string.Join(", ", values.Select(value => $"'{value}'")));

        private bool ValidateAllowedValues(
            string targetWitName,
            string sourceFieldReferenceName,
            List<string> sourceAllowedValues,
            string targetFieldReferenceName,
            List<string> targetAllowedValues,
            out List<string> missingValues,
            LogLevel logLevel)
        {
            missingValues = sourceAllowedValues
                .Except(targetAllowedValues, StringComparer.OrdinalIgnoreCase)
                .ToList();
            if (missingValues.Count > 0)
            {
                Log.LogDebug("    Allowed values in target field do not match allowed values in source. Checking field value maps.");
                LogAllowedValues("    Missing values are: {missingValues}", missingValues);
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
                            Log.LogDebug("      Value '{missingValue}' is mapped to '{mappedValue}', which exists in target.",
                                missingValue, mappedValue);
                        }
                        else
                        {
                            Log.Log(logLevel, "      Value '{missingValue}' is mapped to '{mappedValue}', which does not exists in target."
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
            allowedValues.Sort(StringComparer.OrdinalIgnoreCase);
            return (valueType, allowedValues);
        }

        private bool ShouldValidateWorkItemType(string workItemTypeName)
        {
            if ((Options.IncludeWorkItemTypes.Count > 0)
                && !Options.IncludeWorkItemTypes.Contains(workItemTypeName, StringComparer.OrdinalIgnoreCase))
            {
                Log.LogInformation(
                    "Skipping validation of work item type '{sourceWit}' because it is not included in validation list.",
                    workItemTypeName);
                return false;
            }
            else if ((Options.ExcludeWorkItemTypes.Count > 0)
                && Options.ExcludeWorkItemTypes.Contains(workItemTypeName, StringComparer.OrdinalIgnoreCase))
            {
                Log.LogInformation(
                    "Skipping validation of work item type '{sourceWit}' because it is excluded from validation list.",
                    workItemTypeName);
                return false;
            }
            return true;
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

            Log.LogError("Some work item types or their fields are not valid in the target system (see previous logs).");

            Log.LogInformation("If the work item type does not exist in target system, you can:");
            Log.LogInformation("  - Create it there.");
            Log.LogInformation("  - Configure mapping to another work item type which exists in target using"
                + $" '{nameof(WorkItemTypeMappingTool)}' configuration.");
            Log.LogInformation("  - Exclude it from validation. To configure which work item types are validated, use either"
                + $" '{nameof(TfsWorkItemTypeValidatorToolOptions.IncludeWorkItemTypes)}' or"
                + $" '{nameof(TfsWorkItemTypeValidatorToolOptions.ExcludeWorkItemTypes)}'"
                + $" of '{nameof(TfsWorkItemTypeValidatorTool)}' configuration (but not both at the same time).");

            Log.LogInformation("If field is missing in target, you can:");
            Log.LogInformation("  - Add missing field to the target work item type.");
            Log.LogInformation($"  - Configure field mapping using '{nameof(FieldToFieldMultiMap)}'"
                + $" in '{nameof(FieldMappingTool)}' configuration."
                + " This is simpler method for source to target field mapping, which allows to map multiple fields at once.");
            Log.LogInformation($"  - Configure field mapping using '{nameof(FieldToFieldMap)}'"
                + $" in '{nameof(FieldMappingTool)}' configuration."
                + $" Mapping mode must be of type '{nameof(FieldMapMode.SourceToTarget)}'.");

            Log.LogInformation("If allowed values of the source and target fields do not match, you can:");
            Log.LogInformation("  - Add missing allowed values to the target field.");
            Log.LogInformation($"  - Configure value mapping. Add field maps of type '{nameof(FieldValueMap)}'"
                + $" to '{nameof(FieldMappingTool)}' configuration.");

            Log.LogInformation("To exclude field from validation, just configure it in"
                + $" '{nameof(TfsWorkItemTypeValidatorToolOptions.ExcludeSourceFields)}'"
                + $" of '{nameof(TfsWorkItemTypeValidatorTool)}' configuration."
                + " If field is excluded from validation, all the issues are still logged, just the result of validation is 'valid'.");
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
