using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;
using MigrationTools.DataContracts;
using MigrationTools.Enrichers;

namespace MigrationTools.Endpoints
{
    public abstract class WorkItemEndpoint : IWorkItemSourceEndPoint, IWorkItemTargetEndPoint
    {
        protected List<WorkItemData> _innerList = new List<WorkItemData>();
        protected IWorkItemQuery _WorkItemStoreQuery;
        protected List<IWorkItemEnricher> _innerEnrichers = new List<IWorkItemEnricher>();

        public WorkItemEndpoint(IOptions<EndpointOptions> endpointOptions)
        {
            EndpointOptions = endpointOptions.Value;
        }

        public int Count { get { return _innerList.Count; } }

        public IEnumerable<WorkItemData> WorkItems { get { return _innerList; } }

        public IEnumerable<IWorkItemSourceEnricher> SourceEnrichers => (from e in _innerEnrichers where e is IWorkItemSourceEnricher select (IWorkItemSourceEnricher)e);

        public IEnumerable<IWorkItemTargetEnricher> TargetEnrichers => (from e in _innerEnrichers where e is IWorkItemTargetEnricher select (IWorkItemTargetEnricher)e);

        public IEndpointOptions EndpointOptions { get; }

        public virtual void Configure(IWorkItemQuery query, List<IWorkItemEnricher> enrichers)
        {
            _WorkItemStoreQuery = query ?? throw new ArgumentNullException(nameof(query));
            if (enrichers != null)
            {
                _innerEnrichers = enrichers;
            }

            RefreshStore();
        }

        public virtual void Filter(IEnumerable<WorkItemData> workItems)
        {
            var ids = (from x in workItems.ToList() select x.Id);
            _innerList = (from x in _innerList
                          where !ids.Contains(x.Id)
                          select x).ToList();
        }

        public virtual IEnumerable<WorkItemData> GetWorkItems()
        {
            return _innerList;
        }

        public abstract void PersistWorkItem(WorkItemData source);

        protected virtual void RefreshStore()
        {
            _innerList.Clear();
            _innerList.AddRange(_WorkItemStoreQuery.GetWorkItems());
        }
    }
}