using MigrationTools.Core.Configuration;
using MigrationTools.Core.DataContracts;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace MigrationTools.Core.Clients.Tests
{
    class WorkItemSinkStub : IMigrationClient
    {
        List<WorkItemData> list = new List<WorkItemData>();

        public TeamProjectConfig Config => throw new System.NotImplementedException();

        public object InternalCollection => throw new System.NotImplementedException();

        public void Configure(TeamProjectConfig config, NetworkCredential credentials = null)
        {
            throw new System.NotImplementedException();
        }

        public T GetService<T>()
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<WorkItemData> GetWorkItems()
        {
            if (list.Count == 0)
            { 
                PopulateList();
            }
            return list;
        }

        public WorkItemData PersistWorkItem(WorkItemData workItem)
        {
            PopulateList();
            var found = list.Find(x => x.id == workItem.id);
            if (found != null)
            {
                // Add Revission
                found.Title = workItem.Title;
                return workItem;
            } else
            {
                // Create new
                var newid = list.Max(s => int.Parse(s.id)) + 1;
                list.Add(new WorkItemData { id = newid.ToString(), Title = workItem.Title });
                return workItem;
            }
        }

        private void PopulateList()
        {
            list.Clear();
            list.Add(new WorkItemData { id = "1", Title = "Item 1" });
            list.Add(new WorkItemData { id = "2", Title = "Item 2" });
            list.Add(new WorkItemData { id = "3", Title = "Item 3" });
            list.Add(new WorkItemData { id = "4", Title = "Item 4" });
            list.Add(new WorkItemData { id = "5", Title = "Item 5" });
        }
    }
}
