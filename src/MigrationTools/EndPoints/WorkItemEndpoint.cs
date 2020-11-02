using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using MigrationTools.DataContracts;
using MigrationTools.Enrichers;

namespace MigrationTools.Endpoints
{
    public abstract class WorkItemEndpoint : IWorkItemSourceEndPoint, IWorkItemTargetEndPoint
    {
        private IEndpointOptions _EndpointOptions;
        protected List<WorkItemData2> _innerList = new List<WorkItemData2>();
        protected IWorkItemQuery _WorkItemStoreQuery;
        protected List<IWorkItemProcessorEnricher> _innerEnrichers = new List<IWorkItemProcessorEnricher>();

        public WorkItemEndpoint(IServiceProvider services, ITelemetryLogger telemetry, ILogger<WorkItemEndpoint> logger)
        {
            Services = services;
            Telemetry = telemetry;
            Log = logger;
        }

        public int Count { get { return _innerList.Count; } }

        public IEnumerable<WorkItemData2> WorkItems { get { return _innerList; } }

        public IEnumerable<IWorkItemProcessorSourceEnricher> SourceEnrichers => (from e in _innerEnrichers where e is IWorkItemProcessorSourceEnricher select (IWorkItemProcessorSourceEnricher)e);

        public IEnumerable<IWorkItemProcessorTargetEnricher> TargetEnrichers => (from e in _innerEnrichers where e is IWorkItemProcessorTargetEnricher select (IWorkItemProcessorTargetEnricher)e);

        public IEndpointOptions EndpointOptions => _EndpointOptions;

        protected IServiceProvider Services { get; }
        protected ITelemetryLogger Telemetry { get; }
        protected ILogger<WorkItemEndpoint> Log { get; }

        public virtual void Configure(IWorkItemQuery query, List<IWorkItemProcessorEnricher> enrichers)
        {
            _WorkItemStoreQuery = query ?? throw new ArgumentNullException(nameof(query));
            if (enrichers != null)
            {
                _innerEnrichers = enrichers;
            }

            RefreshStore();
        }

        public void Configure(IEndpointOptions options)
        {
            _EndpointOptions = options;
        }

        public virtual void Filter(IEnumerable<WorkItemData2> workItems)
        {
            var ids = (from x in workItems.ToList() select x.Id);
            _innerList = (from x in _innerList
                          where !ids.Contains(x.Id)
                          select x).ToList();
        }

        public virtual IEnumerable<WorkItemData2> GetWorkItems()
        {
            return _innerList;
        }

        public abstract void PersistWorkItem(WorkItemData2 source);

        protected virtual void RefreshStore()
        {
            _innerList.Clear();
            _innerList.AddRange(_WorkItemStoreQuery.GetWorkItems2());
        }
    }
}