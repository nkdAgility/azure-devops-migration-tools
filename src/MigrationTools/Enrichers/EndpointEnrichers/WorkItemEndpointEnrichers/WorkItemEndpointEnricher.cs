using System;
using Microsoft.Extensions.Logging;

namespace MigrationTools.Enrichers
{
    public abstract class WorkItemEndpointEnricher : IWorkItemEndpointEnricher
    {
        public WorkItemEndpointEnricher(IServiceProvider services, ITelemetryLogger telemetry, ILogger<WorkItemEndpointEnricher> logger)
        {
            Services = services;
            Telemetry = telemetry;
            Log = logger;
        }

        public IServiceProvider Services { get; }
        public ITelemetryLogger Telemetry { get; }
        public ILogger<WorkItemEndpointEnricher> Log { get; }

        public abstract void Configure(IEndpointEnricherOptions options);
    }
}