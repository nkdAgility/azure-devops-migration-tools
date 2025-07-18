using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using MigrationTools.Tools.Infrastructure;
using MigrationTools.Tools.Interfaces;

namespace MigrationTools.Tools
{
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

        public bool ValidateWorkItemTypes(
            List<WorkItemType> sourceWits,
            List<WorkItemType> targetWits,
            string reflectedWorkItemIdField)
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
                    if (!ValidateReflectedWorkItemIdField(targetWit, reflectedWorkItemIdField))
                    {
                        isValid = false;
                    }
                    if (!ValidateWorkItemTypeFields(sourceWit, targetWit))
                    {
                        isValid = false;
                    }
                }
            }
            LogValidationResult(isValid);
            return isValid;
        }

        private bool ValidateReflectedWorkItemIdField(WorkItemType targetWit, string reflectedWorkItemIdField)
        {
            if (targetWit.FieldDefinitions.Contains(reflectedWorkItemIdField))
            {
                Log.LogDebug("  '{targetWit}' contains reflected work item ID field '{fieldName}'.",
                    targetWit.Name, reflectedWorkItemIdField);
            }
            else
            {
                Log.LogWarning("  '{targetWit}' does not contain reflected work item ID field '{fieldName}'.",
                    targetWit.Name, reflectedWorkItemIdField);
                return false;
            }
            return true;
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
                    Log.LogDebug("  Source field '{sourceFieldName}' exists in '{targetWit}'.", sourceFieldName, targetWit.Name);
                }
                else
                {
                    Log.LogWarning("  Missing field '{targetFieldName}' in '{targetWit}'.", targetFieldName, targetWit.Name);
                    Log.LogInformation("    Source field name: {sourceFieldName}", sourceFieldName);
                    Log.LogInformation("    Field type: {fieldType}", sourceField.FieldType);
                    IEnumerable<string> allowedValues = sourceField.AllowedValues.OfType<string>().Select(val => $"'{val}'");
                    Log.LogInformation("    Allowed values: {allowedValues}", string.Join(", ", allowedValues));
                    result = false;
                }
            }

            if (result)
            {
                Log.LogInformation("  All fields are either present or mapped.");
            }
            return result;
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

        private void LogValidationResult(bool isValid)
        {
            if (isValid)
            {
                Log.LogInformation("All work item types are valid.");
                return;
            }

            const string message =
                "Some work item types or their fields are not present in the target system (see previous logs)." +
                " Either add these fields into target work items, or map source fields to other target fields" +
                $" in options ({nameof(TfsWorkItemTypeValidatorToolOptions.FieldMappings)}).";
            Log.LogError(message);
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
