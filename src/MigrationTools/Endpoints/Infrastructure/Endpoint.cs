using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using MigrationTools.EndpointEnrichers;

namespace MigrationTools.Endpoints
{
    public abstract class Endpoint<TOptions> : ISourceEndPoint, ITargetEndPoint
        where TOptions : IEndpointOptions
    {
        private List<IEndpointEnricher> _EndpointEnrichers;

        public Endpoint(EndpointEnricherContainer endpointEnrichers, ITelemetryLogger telemetry, ILogger<Endpoint<TOptions>> logger)
        {
            EndpointEnrichers = endpointEnrichers;
            Telemetry = telemetry;
            Log = logger;
            _EndpointEnrichers = new List<IEndpointEnricher>();
        }

        public EndpointEnricherContainer EndpointEnrichers { get; }

        public IEnumerable<IEndpointSourceEnricher> SourceEnrichers => _EndpointEnrichers.Where(e => e.GetType().IsAssignableFrom(typeof(IEndpointSourceEnricher))).Select(e => (IEndpointSourceEnricher)e);
        public IEnumerable<IEndpointTargetEnricher> TargetEnrichers => _EndpointEnrichers.Where(e => e.GetType().IsAssignableFrom(typeof(IEndpointTargetEnricher))).Select(e => (IEndpointTargetEnricher)e);

        protected ITelemetryLogger Telemetry { get; }
        protected ILogger<Endpoint<TOptions>> Log { get; }

        public TOptions Options { get; private set; }

        public abstract int Count { get; }

        public virtual void Configure(TOptions options)
        {
            Log.LogDebug("Endpoint::Configure");
            Options = options;
            EndpointEnrichers.ConfigureEnrichers(Options.EndpointEnrichers);
        }

        //public abstract void Filter(IEnumerable<WorkItemData> workItems);

        //public abstract IEnumerable<WorkItemData> GetWorkItems();

        //public abstract IEnumerable<WorkItemData> GetWorkItems(QueryOptions query);

        //public abstract void PersistWorkItem(WorkItemData sourceWorkItem);
    }
}