using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MigrationTools.DataContracts;
using MigrationTools.EndPoints;
using Newtonsoft.Json;

namespace MigrationTools.Clients.FileSystem.Endpoints
{
    public class WorkItemEndpoint : IWorkItemSourceEndPoint, IWorkItemTargetEndPoint
    {
        private List<WorkItemData> _innerList = new List<WorkItemData>();
        private string _WorkItemStore;

        public WorkItemEndpoint()
        {
        }

        public int Count { get { return _innerList.Count; } }

        public IEnumerable<WorkItemData> WorkItems { get { return _innerList; } }

        public void Configure(IWorkItemQuery query)
        {
            _innerList.Clear();
            _innerList.AddRange(query.GetWorkItems());
            _WorkItemStore = query.Query;
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
            var content = JsonConvert.SerializeObject(source, Formatting.Indented);
            var fileName = Path.Combine(_WorkItemStore, string.Format("{0}.json", source.Id));
            System.IO.File.WriteAllText(fileName, content);
        }
    }
}