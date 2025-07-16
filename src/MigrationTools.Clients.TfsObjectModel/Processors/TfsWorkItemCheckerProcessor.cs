using System;
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
    public class TfsWorkItemCheckerProcessor : TfsProcessor
    {
        public TfsWorkItemCheckerProcessor(
            IOptions<TfsWorkItemCheckerProcessorOptions> options,
            TfsCommonTools tfsCommonTools,
            ProcessorEnricherContainer processorEnrichers,
            IServiceProvider services,
            ITelemetryLogger telemetry,
            ILogger<TfsWorkItemCheckerProcessor> logger)
            : base(options, tfsCommonTools, processorEnrichers, services, telemetry, logger)
        {
        }

        public new TfsWorkItemCheckerProcessorOptions Options => (TfsWorkItemCheckerProcessorOptions)base.Options;

        protected override void InternalExecute()
        {
            ValidateWorkItemTypes(GetAllSourceWorkItemTypes().ToList(), GetAllTargetWorkItemTypes().ToList());
        }

        private IEnumerable<WorkItemType> GetAllSourceWorkItemTypes()
        {
            Log.LogInformation("Retrieving all source work item types.");
            return Source.WorkItems.Project
                .ToProject()
                .WorkItemTypes
                .Cast<WorkItemType>();
        }

        private IEnumerable<WorkItemType> GetAllTargetWorkItemTypes()
        {
            Log.LogInformation("Retrieving all target work item types.");
            return Target.WorkItems.Project
                .ToProject()
                .WorkItemTypes
                .Cast<WorkItemType>();
        }

        private void ValidateWorkItemTypes(ICollection<WorkItemType> sourceWits, ICollection<WorkItemType> targetWits)
        {
            LogWorkItemTypes(sourceWits, targetWits);

            bool allFieldsAreMapped = true;
            foreach (WorkItemType sourceWit in sourceWits)
            {
                Log.LogInformation("Validating fields of work item type '{sourceWit}'", sourceWit.Name);
                string targetWitName = GetTargetWorkItemType(sourceWit.Name);
                WorkItemType targetWit = targetWits
                    .FirstOrDefault(wit => wit.Name.Equals(targetWitName, StringComparison.OrdinalIgnoreCase));
                if (targetWit is null)
                {
                    Log.LogWarning("Work item type '{targetWit}' is not present in target system.", targetWitName);
                    allFieldsAreMapped = false;
                }
                else
                {
                    if (!ValidateWorkItemTypeFields(sourceWit, targetWit))
                    {
                        allFieldsAreMapped = false;
                    }
                }
            }
            if (LogFieldsAreNotMapped(allFieldsAreMapped))
            {
                Environment.Exit(-1);
            }
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
            string targetFieldName = CommonTools.FieldReferenceNameMappingTool
                .GetTargetFieldName(targetWitName, sourceFieldName, out bool isMapped);
            if (isMapped)
            {
                if (string.IsNullOrEmpty(targetFieldName))
                {
                    Log.LogInformation(
                        "  Source field '{sourceFieldName}' is explicitly mapped as empty string, so it is not checked in target.",
                        sourceFieldName, targetFieldName);
                }
                else
                {
                    Log.LogInformation("  Source field '{sourceFieldName}' is mapped as '{targetFieldName}' in target.",
                        sourceFieldName, targetFieldName);
                }
            }
            return targetFieldName;
        }

        private void LogWorkItemTypes(ICollection<WorkItemType> sourceWits, ICollection<WorkItemType> targetWits)
        {
            Log.LogInformation("Validating::Check that all fields in source work items exists in target work items.");
            Log.LogInformation("Validating fields of source work item types: {sourceWits}.",
                string.Join(", ", sourceWits.Select(wit => wit.Name)));
            Log.LogInformation("Available target work item types are: {targetWits}.",
                string.Join(", ", targetWits.Select(wit => wit.Name)));
        }

        private bool LogFieldsAreNotMapped(bool allFieldsAreMapped)
        {
            if (allFieldsAreMapped)
            {
                return false;
            }

            const string message = "Some fields are not present in the target system (see previous logs)." +
                " If the migration will continue, you will not have all information in work items in target system." +
                " Either add these fields into target work items, or map source fields to other target fields" +
                $" using '{nameof(FieldReferenceNameMappingTool)}'.";
            const string opt = nameof(TfsWorkItemCheckerProcessorOptions.StopIfMissingFieldsInTarget);
            if (Options.StopIfMissingFieldsInTarget)
            {
                Log.LogError(message);
                Log.LogInformation($"'{opt}' is set to 'true' so migration process will stop now.");
                return true;
            }
            else
            {
                Log.LogWarning(message);
                Log.LogInformation($"'{opt}' is set to 'false' so migration process will continue. Set it to 'true'" +
                    " if you want to stop and resolve missing fields.");
            }
            return false;
        }
    }
}
