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

        private void Configure(IWorkItemQuery query)
        {
            _innerList.Clear();
            for (int i = 0; i < 10; i++)
            {
                _innerList.Add(new WorkItemData()
                {
                    Id = i.ToString(),
                    Title = string.Format("Title {i}", i),
                    Rev = 1,
                    Revision = 1,
                    Revisions =
                });
            }
        }
    }
}