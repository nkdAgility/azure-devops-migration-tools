using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using MigrationTools.EndpointEnrichers;

namespace MigrationTools.Endpoints
{
    public abstract class Endpoint : ISourceEndPoint, ITargetEndPoint
    {
        private IEndpointOptions _InnerOptions;
        private List<IEndpointEnricher> _EndpointEnrichers;
        public EndpointDirection Direction => _InnerOptions.Direction;

        public Endpoint(EndpointEnricherContainer endpointEnrichers, IServiceProvider services, ITelemetryLogger telemetry, ILogger<Endpoint> logger)
        {
            EndpointEnrichers = endpointEnrichers;
            Services = services;
            Telemetry = telemetry;
            Log = logger;
            _EndpointEnrichers = new List<IEndpointEnricher>();
        }

        public EndpointEnricherContainer EndpointEnrichers { get; }

        public IEnumerable<IEndpointSourceEnricher> SourceEnrichers => _EndpointEnrichers.Where(e => e.GetType().IsAssignableFrom(typeof(IEndpointSourceEnricher))).Select(e => (IEndpointSourceEnricher)e);
        public IEnumerable<IEndpointTargetEnricher> TargetEnrichers => _EndpointEnrichers.Where(e => e.GetType().IsAssignableFrom(typeof(IEndpointTargetEnricher))).Select(e => (IEndpointTargetEnricher)e);

        protected IServiceProvider Services { get; }
        protected ITelemetryLogger Telemetry { get; }
        protected ILogger<Endpoint> Log { get; }

        public abstract int Count { get; }

        public virtual void Configure(IEndpointOptions options)
        {
            Log.LogDebug("Endpoint::Configure");
            _InnerOptions = options;
            EndpointEnrichers.ConfigureEnrichers(_InnerOptions.Enrichers);
        }

        //public abstract void Filter(IEnumerable<WorkItemData> workItems);

        //public abstract IEnumerable<WorkItemData> GetWorkItems();

        //public abstract IEnumerable<WorkItemData> GetWorkItems(QueryOptions query);

        //public abstract void PersistWorkItem(WorkItemData sourceWorkItem);
    }
}