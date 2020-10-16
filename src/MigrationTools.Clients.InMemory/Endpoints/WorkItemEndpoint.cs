using System;
using System.Collections.Generic;
using System.Linq;
using MigrationTools.DataContracts;
using MigrationTools.EndPoints;

namespace MigrationTools.Clients.InMemory.Endpoints
{
    public class WorkItemEndpoint : IWorkItemSourceEndPoint, IWorkItemTargetEndPoint
    {
        private List<WorkItemData> _innerList = new List<WorkItemData>();

        public WorkItemEndpoint()
        {
        }

        public int Count { get { return _innerList.Count; } }

        public IEnumerable<WorkItemData> WorkItems { get { return _innerList; } }

        public void Configure(IWorkItemQuery query)
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
                CreateNewFrom(source);
            }
            else
            {
                UpdateWorkItemFrom(found, source);
            }
        }

        private void UpdateWorkItemFrom(WorkItemData source, WorkItemData target)
        {
            _innerList.Remove(source);
            _innerList.Add(target);
        }

        public void CreateNewFrom(WorkItemData source)
        {
            _innerList.Add(source);
        }
    }
}