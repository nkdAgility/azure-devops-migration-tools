using System.Collections.Generic;
using System.Linq;
using MigrationTools.Configuration;
using MigrationTools.DataContracts;

namespace MigrationTools.Clients.Tests
{
    internal class WorkItemMigrationClientMock : IWorkItemMigrationClient
    {
        private List<WorkItemData> list = new List<WorkItemData>();

        public IMigrationClientConfig Config => throw new System.NotImplementedException();

        public ProjectData Project => throw new System.NotImplementedException();

        public void Configure(IMigrationClient migrationClient, bool bypassRules = true)
        {
        }

        public List<WorkItemData> GetWorkItems()
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
            var found = list.Find(x => x.Id == workItem.Id);
            if (found != null)
            {
                // Add Revission
                found.Title = workItem.Title;
                return workItem;
            }
            else
            {
                // Create new
                var newid = list.Max(s => int.Parse(s.Id)) + 1;
                list.Add(new WorkItemData { Id = newid.ToString(), Title = workItem.Title });
                return workItem;
            }
        }

        private void PopulateList()
        {
            list.Clear();
            list.Add(new WorkItemData { Id = "1", Title = "Item 1" });
            list.Add(new WorkItemData { Id = "2", Title = "Item 2" });
            list.Add(new WorkItemData { Id = "3", Title = "Item 3" });
            list.Add(new WorkItemData { Id = "4", Title = "Item 4" });
            list.Add(new WorkItemData { Id = "5", Title = "Item 5" });
        }

        public IEnumerable<WorkItemData> GetWorkItems(string query)
        {
            throw new System.NotImplementedException();
        }

        public string CreateReflectedWorkItemId(WorkItemData wi)
        {
            throw new System.NotImplementedException();
        }

        public WorkItemData FindReflectedWorkItem(WorkItemData workItem, bool cache)
        {
            throw new System.NotImplementedException();
        }

        public WorkItemData FindReflectedWorkItemByMigrationRef(string refId)
        {
            throw new System.NotImplementedException();
        }

        public WorkItemData FindReflectedWorkItemByReflectedWorkItemId(string refId)
        {
            throw new System.NotImplementedException();
        }

        public WorkItemData FindReflectedWorkItemByReflectedWorkItemId(int refId, bool cache)
        {
            throw new System.NotImplementedException();
        }

        public WorkItemData FindReflectedWorkItemByReflectedWorkItemId(WorkItemData refWi)
        {
            throw new System.NotImplementedException();
        }

        public WorkItemData FindReflectedWorkItemByTitle(string title)
        {
            throw new System.NotImplementedException();
        }

        public int GetReflectedWorkItemId(WorkItemData workItem)
        {
            throw new System.NotImplementedException();
        }

        public WorkItemData GetRevision(WorkItemData workItem, int revision)
        {
            throw new System.NotImplementedException();
        }

        public ProjectData GetProject()
        {
            throw new System.NotImplementedException();
        }

        public WorkItemData GetWorkItem(string id)
        {
            throw new System.NotImplementedException();
        }

        List<WorkItemData> IWorkItemMigrationClient.GetWorkItems(string query)
        {
            throw new System.NotImplementedException();
        }

        public List<WorkItemData> FilterWorkItemsThatAlreadyExist(List<WorkItemData> sourceWorkItems, IWorkItemMigrationClient target)
        {
            throw new System.NotImplementedException();
        }

        public WorkItemData GetWorkItem(int id)
        {
            throw new System.NotImplementedException();
        }

        public List<WorkItemData> GetWorkItems(IWorkItemQueryBuilder queryBuilder)
        {
            throw new System.NotImplementedException();
        }

        public WorkItemData FindReflectedWorkItemByMigrationRef(ReflectedWorkItemId refId)
        {
            throw new System.NotImplementedException();
        }

        public WorkItemData FindReflectedWorkItemByReflectedWorkItemId(ReflectedWorkItemId refId, bool cache)
        {
            throw new System.NotImplementedException();
        }

        ReflectedWorkItemId IWorkItemMigrationClient.CreateReflectedWorkItemId(WorkItemData workItem)
        {
            throw new System.NotImplementedException();
        }

        ReflectedWorkItemId IWorkItemMigrationClient.GetReflectedWorkItemId(WorkItemData workItem)
        {
            throw new System.NotImplementedException();
        }
    }
}