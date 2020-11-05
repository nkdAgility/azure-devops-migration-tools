using System.Collections.Generic;
using System.Linq;
using MigrationTools._Enginev1.DataContracts;
using MigrationTools.Configuration;
using MigrationTools.DataContracts;

namespace MigrationTools.Clients.Tests
{
    internal class WorkItemMigrationClientMock : IWorkItemMigrationClient
    {
        private List<_Enginev1.DataContracts.WorkItemData> list = new List<_Enginev1.DataContracts.WorkItemData>();

        public IMigrationClientConfig Config => throw new System.NotImplementedException();

        public ProjectData Project => throw new System.NotImplementedException();

        public void Configure(IMigrationClient migrationClient, bool bypassRules = true)
        {
        }

        public List<_Enginev1.DataContracts.WorkItemData> GetWorkItems()
        {
            if (list.Count == 0)
            {
                PopulateList();
            }
            return list;
        }

        public _Enginev1.DataContracts.WorkItemData PersistWorkItem(_Enginev1.DataContracts.WorkItemData workItem)
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
                list.Add(new _Enginev1.DataContracts.WorkItemData { Id = newid.ToString(), Title = workItem.Title });
                return workItem;
            }
        }

        private void PopulateList()
        {
            list.Clear();
            list.Add(new _Enginev1.DataContracts.WorkItemData { Id = "1", Title = "Item 1" });
            list.Add(new _Enginev1.DataContracts.WorkItemData { Id = "2", Title = "Item 2" });
            list.Add(new _Enginev1.DataContracts.WorkItemData { Id = "3", Title = "Item 3" });
            list.Add(new _Enginev1.DataContracts.WorkItemData { Id = "4", Title = "Item 4" });
            list.Add(new _Enginev1.DataContracts.WorkItemData { Id = "5", Title = "Item 5" });
        }

        public IEnumerable<_Enginev1.DataContracts.WorkItemData> GetWorkItems(string query)
        {
            throw new System.NotImplementedException();
        }

        public string CreateReflectedWorkItemId(_Enginev1.DataContracts.WorkItemData wi)
        {
            throw new System.NotImplementedException();
        }

        public _Enginev1.DataContracts.WorkItemData FindReflectedWorkItem(_Enginev1.DataContracts.WorkItemData workItem, bool cache)
        {
            throw new System.NotImplementedException();
        }

        public _Enginev1.DataContracts.WorkItemData FindReflectedWorkItemByMigrationRef(string refId)
        {
            throw new System.NotImplementedException();
        }

        public _Enginev1.DataContracts.WorkItemData FindReflectedWorkItemByReflectedWorkItemId(string refId)
        {
            throw new System.NotImplementedException();
        }

        public _Enginev1.DataContracts.WorkItemData FindReflectedWorkItemByReflectedWorkItemId(int refId, bool cache)
        {
            throw new System.NotImplementedException();
        }

        public _Enginev1.DataContracts.WorkItemData FindReflectedWorkItemByReflectedWorkItemId(_Enginev1.DataContracts.WorkItemData refWi)
        {
            throw new System.NotImplementedException();
        }

        public _Enginev1.DataContracts.WorkItemData FindReflectedWorkItemByTitle(string title)
        {
            throw new System.NotImplementedException();
        }

        public int GetReflectedWorkItemId(_Enginev1.DataContracts.WorkItemData workItem)
        {
            throw new System.NotImplementedException();
        }

        public _Enginev1.DataContracts.WorkItemData GetRevision(_Enginev1.DataContracts.WorkItemData workItem, int revision)
        {
            throw new System.NotImplementedException();
        }

        public ProjectData GetProject()
        {
            throw new System.NotImplementedException();
        }

        public _Enginev1.DataContracts.WorkItemData GetWorkItem(string id)
        {
            throw new System.NotImplementedException();
        }

        List<_Enginev1.DataContracts.WorkItemData> IWorkItemMigrationClient.GetWorkItems(string query)
        {
            throw new System.NotImplementedException();
        }

        public List<_Enginev1.DataContracts.WorkItemData> FilterWorkItemsThatAlreadyExist(List<_Enginev1.DataContracts.WorkItemData> sourceWorkItems, IWorkItemMigrationClient target)
        {
            throw new System.NotImplementedException();
        }

        public _Enginev1.DataContracts.WorkItemData GetWorkItem(int id)
        {
            throw new System.NotImplementedException();
        }

        public List<_Enginev1.DataContracts.WorkItemData> GetWorkItems(IWorkItemQueryBuilder queryBuilder)
        {
            throw new System.NotImplementedException();
        }

        public _Enginev1.DataContracts.WorkItemData FindReflectedWorkItemByMigrationRef(ReflectedWorkItemId refId)
        {
            throw new System.NotImplementedException();
        }

        public _Enginev1.DataContracts.WorkItemData FindReflectedWorkItemByReflectedWorkItemId(ReflectedWorkItemId refId, bool cache)
        {
            throw new System.NotImplementedException();
        }

        ReflectedWorkItemId IWorkItemMigrationClient.CreateReflectedWorkItemId(_Enginev1.DataContracts.WorkItemData workItem)
        {
            throw new System.NotImplementedException();
        }

        ReflectedWorkItemId IWorkItemMigrationClient.GetReflectedWorkItemId(_Enginev1.DataContracts.WorkItemData workItem)
        {
            throw new System.NotImplementedException();
        }
    }
}