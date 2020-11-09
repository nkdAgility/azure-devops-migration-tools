using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using MigrationTools.DataContracts;
using MigrationTools.Enrichers;
using MigrationTools.Options;

namespace MigrationTools.Endpoints
{
    public abstract class WorkItemEndpoint : IWorkItemSourceEndPoint, IWorkItemTargetEndPoint
    {
        public WorkItemEndpoint(EndpointEnricherContainer endpointEnrichers, IServiceProvider services, ITelemetryLogger telemetry, ILogger<WorkItemEndpoint> logger)
        {
            EndpointEnrichers = endpointEnrichers;
            Services = services;
            Telemetry = telemetry;
            Log = logger;
        }

        public EndpointEnricherContainer EndpointEnrichers { get; }
        public abstract IEnumerable<IWorkItemProcessorSourceEnricher> SourceEnrichers { get; }
        public abstract int Count { get; }
        public abstract EndpointDirection Direction { get; }
        public abstract IEnumerable<IWorkItemProcessorTargetEnricher> TargetEnrichers { get; }
        protected IServiceProvider Services { get; }
        protected ITelemetryLogger Telemetry { get; }
        protected ILogger<WorkItemEndpoint> Log { get; }

        public abstract void Configure(IEndpointOptions options);

        public abstract void Filter(IEnumerable<WorkItemData> workItems);

        public abstract IEnumerable<WorkItemData> GetWorkItems();

        public abstract IEnumerable<WorkItemData> GetWorkItems(QueryOptions query);

        public abstract void PersistWorkItem(WorkItemData sourceWorkItem);
    }
}