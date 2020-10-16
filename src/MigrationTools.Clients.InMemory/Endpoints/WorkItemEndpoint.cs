using System.Collections.Generic;
using MigrationTools.DataContracts;
using MigrationTools.EndPoints;

namespace MigrationTools.Clients.InMemory.Endpoints
{
    public class WorkItemEndpoint : IWorkItemEndpoint
    {
        private List<WorkItemData> _innerList = new List<WorkItemData>();

        public WorkItemEndpoint()
        {
        }

        public int Count { get { return _innerList.Count; } }

        public void Configure(IWorkItemQuery query)
        {
            _innerList.Clear();
            _innerList.AddRange(query.GetWorkItems());
        }
    }
}