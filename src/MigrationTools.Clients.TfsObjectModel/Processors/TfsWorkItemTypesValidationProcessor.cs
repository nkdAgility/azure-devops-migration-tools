﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using MigrationTools.Enrichers;
using MigrationTools.Processors.Infrastructure;
using MigrationTools.Tools;

namespace MigrationTools.Processors
{
    /// <summary>
    /// Processor for validating work item types in source againts work item types in target.
    /// It checks if all work item types and their fields in source exist in target. It respects work item type mappings
    /// (<see cref="WorkItemTypeMappingTool"/>) and can have field mappings configured in options.
    /// </summary>
    public class TfsWorkItemTypesValidationProcessor : TfsProcessor
    {
        public TfsWorkItemTypesValidationProcessor(
            IOptions<TfsWorkItemTypesValidationProcessorOptions> options,
            TfsCommonTools tfsCommonTools,
            ProcessorEnricherContainer processorEnrichers,
            IServiceProvider services,
            ITelemetryLogger telemetry,
            ILogger<TfsWorkItemTypesValidationProcessor> logger)
            : base(options, tfsCommonTools, processorEnrichers, services, telemetry, logger)
        {
            options.Value.Normalize();
        }

        public new TfsWorkItemTypesValidationProcessorOptions Options => (TfsWorkItemTypesValidationProcessorOptions)base.Options;

        protected override void InternalExecute()
        {
            ValidateWorkItemTypes(GetAllSourceWorkItemTypes(), GetAllTargetWorkItemTypes());
        }

        private List<WorkItemType> GetAllSourceWorkItemTypes()
        {
            Log.LogInformation("Retrieving all source work item types.");
            return Source.WorkItems.Project
                .ToProject()
                .WorkItemTypes
                .Cast<WorkItemType>()
                .ToList();
        }

        private List<WorkItemType> GetAllTargetWorkItemTypes()
        {
            Log.LogInformation("Retrieving all target work item types.");
            return Target.WorkItems.Project
                .ToProject()
                .WorkItemTypes
                .Cast<WorkItemType>()
                .ToList();
        }

        private void ValidateWorkItemTypes(List<WorkItemType> sourceWits, List<WorkItemType> targetWits)
        {
            LogWorkItemTypes(sourceWits, targetWits);

            bool isValid = true;
            foreach (WorkItemType sourceWit in sourceWits)
            {
                if (!ShouldValidateWorkItemType(sourceWit.Name))
                {
                    continue;
                }
                Log.LogInformation("Validating fields of work item type '{sourceWit}'", sourceWit.Name);
                string targetWitName = GetTargetWorkItemType(sourceWit.Name);
                WorkItemType targetWit = targetWits
                    .FirstOrDefault(wit => wit.Name.Equals(targetWitName, StringComparison.OrdinalIgnoreCase));
                if (targetWit is null)
                {
                    Log.LogWarning("Work item type '{targetWit}' is not present in target system.", targetWitName);
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
            StopIfRequested(isValid);
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
                    Log.LogWarning("  Missing field '{targetFieldName}' in '{targetWit}'", targetFieldName, targetWit.Name);
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
            if ((Options.ExcludeWorkItemtypes.Count > 0)
                && Options.ExcludeWorkItemtypes.Contains(workItemTypeName, StringComparer.OrdinalIgnoreCase))
            {
                Log.LogInformation(
                    "Skipping validation of work item type '{sourceWit}' because it is excluded from validation.",
                    workItemTypeName);
                return false;
            }

            return true;
        }

        private string GetTargetWorkItemType(string sourceWit)
        {
            string targetWit = sourceWit;
            if (CommonTools.WorkItemTypeMapping.Mappings.ContainsKey(sourceWit))
            {
                targetWit = CommonTools.WorkItemTypeMapping.Mappings[sourceWit];
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
            Log.LogInformation("Validating work item types if they exist in target system with all properties in source system.");
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

            const string message = "Some fields are not present in the target system (see previous logs)." +
                " If the migration will continue, you will not have all information in work items in target system." +
                " Either add these fields into target work items, or map source fields to other target fields" +
                $" in options ({nameof(TfsWorkItemTypesValidationProcessorOptions.TargetFieldsMappings)}).";
            const string opt = nameof(TfsWorkItemTypesValidationProcessorOptions.StopIfMissingFieldsInTarget);
            if (Options.StopIfMissingFieldsInTarget)
            {
                Log.LogError(message);
                Log.LogInformation($"'{opt}' is set to 'true' so migration process will stop now.");
            }
            else
            {
                Log.LogWarning(message);
                Log.LogInformation($"'{opt}' is set to 'false' so migration process will continue. Set it to 'true'" +
                    " if you want to stop and resolve missing fields.");
            }
        }

        private void StopIfRequested(bool isValid)
        {
            if (!isValid && Options.StopIfMissingFieldsInTarget)
            {
                Environment.Exit(-1);
            }
        }
    }
}
