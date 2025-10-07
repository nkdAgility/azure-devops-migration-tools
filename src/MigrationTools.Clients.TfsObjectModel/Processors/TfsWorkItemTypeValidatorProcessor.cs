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
    /// Work item type validation processor. Basically it just runs the <see cref="TfsWorkItemTypeValidatorTool"/>
    /// to validate work item types. The validation is run always, even if the tool iself is disabled.
    /// Neither this processor, nor the tool do not perform any changes to the source or target system.
    /// </summary>
    public class TfsWorkItemTypeValidatorProcessor : TfsProcessor
    {
        public TfsWorkItemTypeValidatorProcessor(
            IOptions<TfsWorkItemTypeValidatorProcessorOptions> options,
            TfsCommonTools tfsCommonTools,
            ProcessorEnricherContainer processorEnrichers,
            IServiceProvider services,
            ITelemetryLogger telemetry,
            ILogger<TfsWorkItemTypeValidatorProcessor> logger)
            : base(options, tfsCommonTools, processorEnrichers, services, telemetry, logger)
        {
        }

        public new TfsWorkItemTypeValidatorProcessorOptions Options => (TfsWorkItemTypeValidatorProcessorOptions)base.Options;

        protected override void InternalExecute()
        {
            if (!CommonTools.WorkItemTypeValidatorTool.Enabled)
            {
                Log.LogInformation("Work item type validation tool is disabled, but this processor will still"
                    + " run the validation. No changes are done to source or target system.");
            }
            List<WorkItemType> sourceWits = Source.WorkItems.Project
                .ToProject()
                .WorkItemTypes
                .Cast<WorkItemType>()
                .OrderBy(wit => wit.Name)
                .ToList();
            List<WorkItemType> targetWits = Target.WorkItems.Project
                .ToProject()
                .WorkItemTypes
                .Cast<WorkItemType>()
                .OrderBy(wit => wit.Name)
                .ToList();
            bool containsReflectedWorkItemId = CommonTools.WorkItemTypeValidatorTool
                .ValidateReflectedWorkItemIdField(sourceWits, targetWits, Target.Options.ReflectedWorkItemIdField);
            bool validationResult = CommonTools.WorkItemTypeValidatorTool
                .ValidateWorkItemTypes(sourceWits, targetWits);
            if ((Options.StopIfValidationFails && !validationResult) || (!containsReflectedWorkItemId))
            {
                const string message =
                    "Either the reflected work item type ID field '{ReflectedWorkItemIdField}' is missing in some of the target work item types"
                    + $" or '{nameof(Options.StopIfValidationFails)}' is set to 'true', so migration process will stop now.";
                Log.LogInformation(message, Target.Options.ReflectedWorkItemIdField);
                Environment.Exit(-1);
            }
        }
    }
}
