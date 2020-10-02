using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using MigrationTools.Configuration;
using MigrationTools.DataContracts;

namespace MigrationTools.Clients
{
    public interface IWorkItemMigrationClient
    {
        TeamProjectConfig Config { get; }
        ProjectData Project { get; }

        void Configure(IMigrationClient migrationClient, bool bypassRules = true);

        ProjectData GetProject();
        List<WorkItemData> GetWorkItems();
        WorkItemData GetWorkItem(string id);
        WorkItemData GetWorkItem(int id);
        List<WorkItemData> GetWorkItems(string wiqlQuery);
        List<WorkItemData> GetWorkItems(IWorkItemQueryBuilder queryBuilder);
        WorkItemData PersistWorkItem(WorkItemData workItem);
        WorkItemData GetRevision(WorkItemData workItem, int revision);
        WorkItemData FindReflectedWorkItem(WorkItemData workItem, bool cache, string sourceReflectedWIIdField = null);
        WorkItemData FindReflectedWorkItemByTitle(string title);
        WorkItemData FindReflectedWorkItemByMigrationRef(string refId);
        WorkItemData FindReflectedWorkItemByReflectedWorkItemId(string refId);
        WorkItemData FindReflectedWorkItemByReflectedWorkItemId(int refId, bool cache);
        WorkItemData FindReflectedWorkItemByReflectedWorkItemId(WorkItemData refWi);
        string CreateReflectedWorkItemId(WorkItemData wi);
        int GetReflectedWorkItemId(WorkItemData workItem, string reflectedWotkItemIdField);

    }
}
