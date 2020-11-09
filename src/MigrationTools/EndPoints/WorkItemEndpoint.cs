using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using MigrationTools.DataContracts;
using MigrationTools.EndpointEnrichers;
using MigrationTools.Options;

namespace MigrationTools.Endpoints
{
    public abstract class WorkItemEndpoint : IWorkItemSourceEndPoint, IWorkItemTargetEndPoint
    {
        private IEndpointOptions _InnerOptions;
        private List<IEndpointEnricher> _EndpointEnrichers;
        public EndpointDirection Direction => _InnerOptions.Direction;

        public WorkItemEndpoint(EndpointEnricherContainer endpointEnrichers, IServiceProvider services, ITelemetryLogger telemetry, ILogger<WorkItemEndpoint> logger)
        {
            EndpointEnrichers = endpointEnrichers;
            Services = services;
            Telemetry = telemetry;
            Log = logger;
            _EndpointEnrichers = new List<IEndpointEnricher>();
        }

        public EndpointEnricherContainer EndpointEnrichers { get; }

        public IEnumerable<IWorkItemEndpointSourceEnricher> SourceEnrichers => _EndpointEnrichers.Where(e => e.GetType().IsAssignableFrom(typeof(IWorkItemEndpointSourceEnricher))).Select(e => (IWorkItemEndpointSourceEnricher)e);
        public IEnumerable<IWorkItemEndpointTargetEnricher> TargetEnrichers => _EndpointEnrichers.Where(e => e.GetType().IsAssignableFrom(typeof(IWorkItemEndpointTargetEnricher))).Select(e => (IWorkItemEndpointTargetEnricher)e);

        public abstract int Count { get; }

        protected IServiceProvider Services { get; }
        protected ITelemetryLogger Telemetry { get; }
        protected ILogger<WorkItemEndpoint> Log { get; }

        public virtual void Configure(IEndpointOptions options)
        {
            Log.LogDebug("WorkItemEndpoint::Configure");
            _InnerOptions = options;
            EndpointEnrichers.ConfigureEnrichers(_InnerOptions.Enrichers);
        }

        public abstract void Filter(IEnumerable<WorkItemData> workItems);

        public abstract IEnumerable<WorkItemData> GetWorkItems();

        public abstract IEnumerable<WorkItemData> GetWorkItems(QueryOptions query);

        public abstract void PersistWorkItem(WorkItemData sourceWorkItem);
    }
}