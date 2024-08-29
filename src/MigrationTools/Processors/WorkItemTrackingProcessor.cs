using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MigrationTools.DataContracts;
using MigrationTools.Endpoints;
using MigrationTools.Enrichers;
using MigrationTools.Options;
using MigrationTools.Processors.Infrastructure;
using MigrationTools.Tools;

namespace MigrationTools.Processors
{
    /// <summary>
    /// This processor is intended, with the aid of [ProcessorEnrichers](../ProcessorEnrichers/index.md), to allow the migration of Work Items between two [Endpoints](../Endpoints/index.md).
    /// </summary>
    public class WorkItemTrackingProcessor : Processor
    {
        private WorkItemTrackingProcessorOptions _config;

        public override ProcessorType Type => ProcessorType.Integrated;

        public WorkItemTrackingProcessor(
                    IOptions<WorkItemTrackingProcessorOptions> options,
                    CommonTools staticTools,
                    ProcessorEnricherContainer processorEnricherContainer,
                    IServiceProvider services,
                    ITelemetryLogger telemetry,
                    ILogger<Processor> logger)
            : base(options, staticTools, processorEnricherContainer, services, telemetry, logger)
        {
            _config = options.Value;    
        }

        protected override void InternalExecute()
        {
            Log.LogInformation("Processor::InternalExecute::Start");
            EnsureConfigured();
            ProcessorEnrichers.ProcessorExecutionBegin(this);
            var source = (IWorkItemSourceEndpoint)Source;
            List<WorkItemData> workItems = source.GetWorkItems().ToList();
            ProcessorEnrichers.ProcessorExecutionAfterSource(this, workItems);
            foreach (WorkItemData item in workItems)
            {
                ProcessorEnrichers.ProcessorExecutionBeforeProcessWorkItem(this, item);
                ProcessWorkItem(item, _config.WorkItemCreateRetryLimit);
                ProcessorEnrichers.ProcessorExecutionAfterProcessWorkItem(this, item);
            }
            ProcessorEnrichers.ProcessorExecutionEnd(this);
            Log.LogInformation("Processor::InternalExecute::End");
        }

        private void ProcessWorkItem(WorkItemData workItem, int workItemCreateRetryLimit)
        {
            Log.LogInformation("Processor::ProcessWorkItem::TheWork");
            // Stuff to really do
        }

        private void EnsureConfigured()
        {
            Log.LogInformation("Processor::EnsureConfigured");
            if (_config == null)
            {
                throw new Exception("You must call Configure() first");
            }
            if (Source is not IEndpoint)
            {
                throw new Exception("The Source endpoint configured must be of type IWorkItemEndpoint");
            }
            if (Target is not IEndpoint)
            {
                throw new Exception("The Target endpoint configured must be of type IWorkItemEndpoint");
            }
        }
    }
}