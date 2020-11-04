using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using MigrationTools.DataContracts;
using MigrationTools.Endpoints;
using MigrationTools.EndPoints;
using MigrationTools.Enrichers;

namespace MigrationTools.Processors
{
    public class WorkItemMigrationProcessor : Processor
    {
        private WorkItemMigrationProcessorOptions _config;

        public WorkItemMigrationProcessor(ProcessorEnricherContainer processorEnricherContainer, EndpointContainer endpointContainer, IServiceProvider services, ITelemetryLogger telemetry, ILogger<Processor> logger) : base(processorEnricherContainer, endpointContainer, services, telemetry, logger)
        {
        }

        public override void Configure(IProcessorOptions config)
        {
            Log.LogInformation("Processor::Configure");
            _config = (WorkItemMigrationProcessorOptions)config;
            Endpoints.ConfigureEndpoints(config.Endpoints);
            Enrichers.ConfigureEnrichers(config.Enrichers);
        }

        protected override void InternalExecute()
        {
            Log.LogInformation("Processor::InternalExecute::Start");
            EnsureConfigured();
            Enrichers.ProcessorExecutionBegin(this);
            var source = (WorkItemEndpoint)Endpoints.Source;
            List<WorkItemData2> workItems = source.GetWorkItems().ToList();
            Enrichers.ProcessorExecutionAfterSource(this, workItems);
            foreach (WorkItemData2 item in workItems)
            {
                Enrichers.ProcessorExecutionBeforeProcessWorkItem(this, item);
                ProcessWorkItem(item, _config.WorkItemCreateRetryLimit);
                Enrichers.ProcessorExecutionAfterProcessWorkItem(this, item);
            }
            Enrichers.ProcessorExecutionEnd(this);
            Log.LogInformation("Processor::InternalExecute::End");
        }

        private void ProcessWorkItem(WorkItemData2 workItem, int workItemCreateRetryLimit)
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
            if (!(Endpoints.Source is WorkItemEndpoint))
            {
                throw new Exception("The Source endpoint configured must be of type WorkItemEndpoint");
            }
            if (!(Endpoints.Target is WorkItemEndpoint))
            {
                throw new Exception("The Target endpoint configured must be of type WorkItemEndpoint");
            }
        }
    }
}