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
        private readonly IWorkItemTypeMappingTool _witMappingTool;

        public TfsWorkItemTypeValidatorTool(
            IOptions<TfsWorkItemTypeValidatorToolOptions> options,
            IWorkItemTypeMappingTool witMappingTool,
            IServiceProvider services,
            ILogger<TfsWorkItemTypeValidatorTool> logger,
            ITelemetryLogger telemetry)
            : base(options, services, logger, telemetry)
        {
            _witMappingTool = witMappingTool ?? throw new ArgumentNullException(nameof(witMappingTool));
            Options.Normalize();
        }

        public bool ValidateReflectedWorkItemIdField(
            List<WorkItemType> sourceWits,
            List<WorkItemType> targetWits,
            string reflectedWorkItemIdField)
        {
            Log.LogInformation("Validating presence of reflected work item ID field '{reflectedWorkItemIdField}'"
                + " in target work item types.", reflectedWorkItemIdField);
            bool isValid = true;
            List<WorkItemType> wits = GetTargetWitsToValidate(sourceWits, targetWits);
            foreach (WorkItemType targetWit in wits)
            {
                if (targetWit.FieldDefinitions.Contains(reflectedWorkItemIdField))
                {
                    Log.LogDebug("  '{targetWit}' contains reflected work item ID field '{fieldName}'.",
                        targetWit.Name, reflectedWorkItemIdField);
                }
                else
                {
                    Log.LogError("  '{targetWit}' does not contain reflected work item ID field '{fieldName}'.",
                        targetWit.Name, reflectedWorkItemIdField);
                    isValid = false;
                }
            }
            LogReflectedWorkItemIdValidationResult(isValid, reflectedWorkItemIdField);
            return isValid;
        }

        private List<WorkItemType> GetTargetWitsToValidate(List<WorkItemType> sourceWits, List<WorkItemType> targetWits)
        {
            List<WorkItemType> targetWitsToValidate = [];
            foreach (WorkItemType sourceWit in sourceWits)
            {
                string sourceWitName = sourceWit.Name;
                if (!ShouldValidateWorkItemType(sourceWitName))
                {
                    continue;
                }
                string targetWitName = GetTargetWorkItemType(sourceWitName);
                WorkItemType targetWit = targetWits
                    .FirstOrDefault(wit => wit.Name.Equals(targetWitName, StringComparison.OrdinalIgnoreCase));
                if (targetWit is not null)
                {
                    targetWitsToValidate.Add(targetWit);
                }
            }
            return targetWitsToValidate;
        }

        public bool ValidateWorkItemTypes(List<WorkItemType> sourceWits, List<WorkItemType> targetWits)
        {
            LogWorkItemTypes(sourceWits, targetWits);

            List<string> targetWitNames = targetWits.Select(wit => wit.Name).ToList();
            bool isValid = true;
            foreach (WorkItemType sourceWit in sourceWits)
            {
                string sourceWitName = sourceWit.Name;
                if (!ShouldValidateWorkItemType(sourceWitName))
                {
                    continue;
                }
                Log.LogInformation("Validating work item type '{sourceWit}'", sourceWitName);
                string targetWitName = GetTargetWorkItemType(sourceWitName);
                WorkItemType targetWit = targetWits
                    .FirstOrDefault(wit => wit.Name.Equals(targetWitName, StringComparison.OrdinalIgnoreCase));
                if (targetWit is null)
                {
                    Log.LogWarning("Work item type '{targetWit}' does not exist in target system.", targetWitName);
                    if (TryFindSimilarWorkItemType(sourceWitName, targetWitNames, out string suggestedName))
                    {
                        Log.LogInformation(" Suggested mapping: '{0}' – '{1}'", sourceWitName, suggestedName);
                    }
                    isValid = false;
                }
                else
                {
                    if (!ValidateWorkItemTypeFields(sourceWit, targetWit))
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
            isValid &= ValidateFieldAllowedValues(sourceField, targetField, logLevel);
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

        private bool ValidateFieldAllowedValues(FieldDefinition sourceField, FieldDefinition targetField, LogLevel logLevel)
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
            if (!DoesTargetContainsAllSourceValues(sourceAllowedValues, targetAllowedValues))
            {
                isValid = false;
                Log.Log(logLevel,
                    "  Source field '{sourceField}' and target field '{targetField}' have different allowed values.",
                    sourceField.ReferenceName, targetField.ReferenceName);
                Log.LogInformation("    Source allowed values: {sourceAllowedValues}",
                    string.Join(", ", sourceAllowedValues.Select(val => $"'{val}'")));
                Log.LogInformation("    Target allowed values: {targetAllowedValues}",
                    string.Join(", ", targetAllowedValues.Select(val => $"'{val}'")));
            }

            return isValid;
        }

        private bool DoesTargetContainsAllSourceValues(List<string> sourceAllowedValues, List<string> targetAllowedValues) =>
            sourceAllowedValues.Except(targetAllowedValues, StringComparer.OrdinalIgnoreCase).Count() == 0;


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

        private string GetTargetFieldName(string targetWitName, string sourceFieldName)
        {
            string targetFieldName = Options.GetTargetFieldName(targetWitName, sourceFieldName, out bool isMapped);
            if (isMapped)
            {
                string message = string.IsNullOrEmpty(targetFieldName)
                    ? "  Source field '{sourceFieldName}' is mapped as empty string, so it is not validated in target."
                    : "  Source field '{sourceFieldName}' is mapped as '{targetFieldName}' in target.";
                Log.LogInformation(message, sourceFieldName, targetFieldName);
            }
            return targetFieldName;
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
