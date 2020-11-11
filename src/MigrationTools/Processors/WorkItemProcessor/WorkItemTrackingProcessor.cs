using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using MigrationTools.DataContracts;
using MigrationTools.Endpoints;
using MigrationTools.Enrichers;

namespace MigrationTools.Processors
{
    public class WorkItemTrackingProcessor : Processor
    {
        private WorkItemMigrationProcessorOptions _config;

        public WorkItemTrackingProcessor(ProcessorEnricherContainer processorEnricherContainer, EndpointContainer endpointContainer, IServiceProvider services, ITelemetryLogger telemetry, ILogger<Processor> logger) : base(processorEnricherContainer, endpointContainer, services, telemetry, logger)
        {
        }

        public override void Configure(IProcessorOptions config)
        {
            Log.LogInformation("Processor::Configure");
            _config = (WorkItemMigrationProcessorOptions)config;
            Endpoints.ConfigureEndpoints(config.Endpoints);
            ProcessorEnrichers.ConfigureEnrichers(config.Enrichers);
        }

        protected override void InternalExecute()
        {
            Log.LogInformation("Processor::InternalExecute::Start");
            EnsureConfigured();
            ProcessorEnrichers.ProcessorExecutionBegin(this);
            var source = (IWorkItemSourceEndPoint)Endpoints.Source;
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
            /// Stuff to really do
        }

        private void EnsureConfigured()
        {
            Log.LogInformation("Processor::EnsureConfigured");
            if (_config == null)
            {
                throw new Exception("You must call Configure() first");
            }
            if (!(Endpoints.Source is Endpoint))
            {
                throw new Exception("The Source endpoint configured must be of type WorkItemEndpoint");
            }
            if (!(Endpoints.Target is Endpoint))
            {
                throw new Exception("The Target endpoint configured must be of type WorkItemEndpoint");
            }
        }
    }
}