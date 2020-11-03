using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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
    }
}