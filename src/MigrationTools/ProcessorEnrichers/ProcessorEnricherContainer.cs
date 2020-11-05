using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MigrationTools.DataContracts;
using MigrationTools.Processors;

namespace MigrationTools.Enrichers
{
    public class ProcessorEnricherContainer : List<IProcessorEnricher>
    {
        public ProcessorEnricherContainer(IServiceProvider services, ITelemetryLogger telemetry, ILogger<ProcessorEnricherContainer> logger)
        {
            Services = services;
            Telemetry = telemetry;
            Log = logger;
        }

        protected ILogger<ProcessorEnricherContainer> Log { get; }
        protected IServiceProvider Services { get; }
        protected ITelemetryLogger Telemetry { get; }

        public void ConfigureEnrichers(List<ProcessorEnricherOptions> enrichers)
        {
            if (enrichers is null)
            {
                Log.LogWarning("No Enrichers have been Configured");
            }
            else
            {
                foreach (IProcessorEnricherOptions item in enrichers)
                {
                    var pe = (WorkItemProcessorEnricher)Services.GetRequiredService(item.ToConfigure);
                    pe.Configure(item);
                    Add(pe);
                    Log.LogInformation("Loading Processor Enricher: {ProcessorEnricherName} {ProcessorEnricherEnabled}", pe.GetType().Name, item.Enabled);
                }
            }
        }

        internal void ProcessorExecutionAfterProcessWorkItem(IProcessor processor, WorkItemData workitem)
        {
            Log.LogInformation("ProcessorEnricherContainer::ProcessorExecutionBeforeProcessWorkItem");
            foreach (var enricher in this)
                enricher.ProcessorExecutionAfterProcessWorkItem(processor, workitem);
        }

        internal void ProcessorExecutionBeforeProcessWorkItem(IProcessor processor, WorkItemData workitem)
        {
            Log.LogInformation("ProcessorEnricherContainer::ProcessorExecutionBeforeProcessWorkItem");
            foreach (var enricher in this)
                enricher.ProcessorExecutionBeforeProcessWorkItem(processor, workitem);
        }

        internal void ProcessorExecutionAfterSource(IProcessor processor, List<WorkItemData> workItems)
        {
            Log.LogInformation("ProcessorEnricherContainer::ProcessorExecutionAfterSource");
            foreach (var enricher in this)
                enricher.ProcessorExecutionAfterSource(processor, workItems);
        }

        public void ProcessorExecutionBegin(IProcessor processor)
        {
            Log.LogInformation("ProcessorEnricherContainer::ProcessorExecutionBegin");
            foreach (var enricher in this)
                enricher.ProcessorExecutionBegin(processor);
        }

        public void ProcessorExecutionEnd(IProcessor processor)
        {
            Log.LogInformation("ProcessorEnricherContainer::ProcessorExecutionEnd");
            foreach (var enricher in this)
                enricher.ProcessorExecutionEnd(processor);
        }
    }
}