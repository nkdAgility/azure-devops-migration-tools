using System;
using System.Collections.Generic;
using System.Linq;
using MigrationTools.DataContracts;
using MigrationTools.EndPoints;
using MigrationTools.Enrichers;

namespace MigrationTools.Clients.InMemory.Endpoints
{
    public class WorkItemEndpoint : IWorkItemSourceEndPoint, IWorkItemTargetEndPoint
    {
        private List<WorkItemData> _innerList = new List<WorkItemData>();
        private List<IWorkItemEnricher> _innerEnrichers = new List<IWorkItemEnricher>();

        public WorkItemEndpoint()
        {
        }

        public int Count { get { return _innerList.Count; } }

        public IEnumerable<IWorkItemSourceEnricher> SourceEnrichers => (from e in _innerEnrichers where e is IWorkItemSourceEnricher select (IWorkItemSourceEnricher)e);

        public IEnumerable<IWorkItemTargetEnricher> TargetEnrichers => (from e in _innerEnrichers where e is IWorkItemTargetEnricher select (IWorkItemTargetEnricher)e);

        public void Configure(IWorkItemQuery query, List<IWorkItemEnricher> enrichers)
        {
            _innerList.Clear();
            _innerList.AddRange(query.GetWorkItems());
        }

        public void Filter(IEnumerable<WorkItemData> workItems)
        {
            var ids = (from x in workItems.ToList() select x.Id);
            _innerList = (from x in _innerList
                          where !ids.Contains(x.Id)
                          select x).ToList();
        }

        public void PersistWorkItem(WorkItemData source)
        {
            var found = (from x in _innerList where x.Id == source.Id select x).SingleOrDefault();
            if (found is null)
            {
                found = CreateNewFrom(source);
            }
            foreach (IWorkItemTargetEnricher enricher in TargetEnrichers)
            {
                enricher.PersistFromWorkItem(source);
            }
            UpdateWorkItemFrom(found, source);
        }

        private void UpdateWorkItemFrom(WorkItemData source, WorkItemData target)
        {
            _innerList.Remove(source);
            _innerList.Add(target);
        }

        public WorkItemData CreateNewFrom(WorkItemData source)
        {
            _innerList.Add(source);
            return source;
        }

        public IEnumerable<WorkItemData> GetWorkItems()
        {
            return _innerList;
        }
    }
}