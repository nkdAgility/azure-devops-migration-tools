using System;
using System.Collections.Generic;
using System.Linq;
using MigrationTools.DataContracts;
using MigrationTools.Enrichers;

namespace MigrationTools.Endpoints
{
    public abstract class WorkItemEndpoint : IWorkItemSourceEndPoint, IWorkItemTargetEndPoint
    {
        protected List<WorkItemData2> _innerList = new List<WorkItemData2>();
        protected IWorkItemQuery _WorkItemStoreQuery;
        protected List<IWorkItemProcessorEnricher> _innerEnrichers = new List<IWorkItemProcessorEnricher>();

        public WorkItemEndpoint(EndpointOptions endpointOptions)
        {
            EndpointOptions = endpointOptions;
        }

        public int Count { get { return _innerList.Count; } }

        public IEnumerable<WorkItemData2> WorkItems { get { return _innerList; } }

        public IEnumerable<IWorkItemProcessorSourceEnricher> SourceEnrichers => (from e in _innerEnrichers where e is IWorkItemProcessorSourceEnricher select (IWorkItemProcessorSourceEnricher)e);

        public IEnumerable<IWorkItemProcessorTargetEnricher> TargetEnrichers => (from e in _innerEnrichers where e is IWorkItemProcessorTargetEnricher select (IWorkItemProcessorTargetEnricher)e);

        public IEndpointOptions EndpointOptions { get; }

        public virtual void Configure(IWorkItemQuery query, List<IWorkItemProcessorEnricher> enrichers)
        {
            _WorkItemStoreQuery = query ?? throw new ArgumentNullException(nameof(query));
            if (enrichers != null)
            {
                _innerEnrichers = enrichers;
            }

            RefreshStore();
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