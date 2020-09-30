using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using MigrationTools.Core.Configuration;
using MigrationTools.Core.DataContracts;

namespace MigrationTools.Core.Clients
{
    public interface IWorkItemMigrationClient
    {
        void Configure(IMigrationClient migrationClient, bool bypassRules = true);
        IEnumerable<WorkItemData> GetWorkItems();
        IEnumerable<WorkItemData> GetWorkItems(string query);
        WorkItemData PersistWorkItem(WorkItemData workItem);
        WorkItemData GetRevision(WorkItemData workItem, int revision);
        WorkItemData FindReflectedWorkItemByTitle(string title);
        WorkItemData FindReflectedWorkItemByMigrationRef(string refId);
        WorkItemData FindReflectedWorkItemByReflectedWorkItemId(string refId);
        WorkItemData FindReflectedWorkItemByReflectedWorkItemId(int refId, bool cache);
        WorkItemData FindReflectedWorkItemByReflectedWorkItemId(WorkItemData refWi);
        string CreateReflectedWorkItemId(WorkItemData wi);
        int GetReflectedWorkItemId(WorkItemData workItem, string reflectedWotkItemIdField);

    }
}
