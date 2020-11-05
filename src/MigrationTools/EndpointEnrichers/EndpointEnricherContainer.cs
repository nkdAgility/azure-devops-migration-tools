using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MigrationTools.Enrichers
{
    public class EndpointEnricherContainer : List<IEndpointEnricher>
    {
        public EndpointEnricherContainer(IServiceProvider services, ITelemetryLogger telemetry, ILogger<EndpointEnricherContainer> logger)
        {
            Services = services;
            Telemetry = telemetry;
            Log = logger;
        }

        protected ILogger<EndpointEnricherContainer> Log { get; }
        protected IServiceProvider Services { get; }
        protected ITelemetryLogger Telemetry { get; }

        public void ConfigureEnrichers(List<EndpointEnricherOptions> enrichers)
        {
            if (enrichers is null)
            {
                Log.LogWarning("No Enrichers have been Configured");
            }
            else
            {
                foreach (IEndpointEnricherOptions item in enrichers)
                {
                    var pe = (WorkItemEndpointEnricher)Services.GetRequiredService(item.ToConfigure);
                    pe.Configure(item);
                    Add(pe);
                    Log.LogInformation("Loading Processor Enricher: {ProcessorEnricherName} {ProcessorEnricherEnabled}", pe.GetType().Name, item.Enabled);
                }
            }
        }
    }
}