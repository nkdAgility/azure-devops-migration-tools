using System;
using Microsoft.Extensions.Logging;
using MigrationTools.DataContracts;
using MigrationTools.Endpoints;

namespace MigrationTools.EndpointEnrichers
{
    public abstract class WorkItemEndpointEnricher : IWorkItemEndpointSourceEnricher, IWorkItemEndpointTargetEnricher
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

        public abstract void EnrichWorkItemData(IEndpoint endpoint, object dataSource, RevisionItem dataTarget);
    }
}