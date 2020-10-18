using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MigrationTools.DataContracts;
using MigrationTools.EndPoints;
using MigrationTools.Enrichers;
using Newtonsoft.Json;

namespace MigrationTools.Clients.FileSystem.Endpoints
{
    public class WorkItemEndpoint : IWorkItemSourceEndPoint, IWorkItemTargetEndPoint
    {
        private List<WorkItemData> _innerList = new List<WorkItemData>();
        private IWorkItemQuery _WorkItemStoreQuery;
        private List<IWorkItemEnricher> _innerEnrichers = new List<IWorkItemEnricher>();

        public WorkItemEndpoint()
        {
        }

        public int Count { get { return _innerList.Count; } }

        public IEnumerable<WorkItemData> WorkItems { get { return _innerList; } }

        public IEnumerable<IWorkItemSourceEnricher> SourceEnrichers => (from e in _innerEnrichers where e is IWorkItemSourceEnricher select (IWorkItemSourceEnricher)e);

        public IEnumerable<IWorkItemTargetEnricher> TargetEnrichers => (from e in _innerEnrichers where e is IWorkItemTargetEnricher select (IWorkItemTargetEnricher)e);

        public void Configure(IWorkItemQuery query, List<IWorkItemEnricher> enrichers)
        {
            _WorkItemStoreQuery = query;
            _innerEnrichers = enrichers;
            RefreshStore();
        }

        public void Filter(IEnumerable<WorkItemData> workItems)
        {
            var ids = (from x in workItems.ToList() select x.Id);
            _innerList = (from x in _innerList
                          where !ids.Contains(x.Id)
                          select x).ToList();
        }

        public IEnumerable<WorkItemData> GetWorkItems()
        {
            return _innerList;
        }

        public void PersistWorkItem(WorkItemData source)
        {
            var content = JsonConvert.SerializeObject(source, Formatting.Indented);
            var fileName = Path.Combine(_WorkItemStoreQuery.Query, string.Format("{0}.json", source.Id));
            File.WriteAllText(fileName, content);
            RefreshStore();
        }

        private void RefreshStore()
        {
            _innerList.Clear();
            _innerList.AddRange(_WorkItemStoreQuery.GetWorkItems());
        }
    }
}