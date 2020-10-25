using System.Collections.Generic;
using MigrationTools.Configuration;
using MigrationTools.DataContracts;

namespace MigrationTools.Clients
{
    public interface IWorkItemMigrationClient
    {
        IMigrationClientConfig Config { get; }
        ProjectData Project { get; }

        void Configure(IMigrationClient migrationClient, bool bypassRules = true);

        ProjectData GetProject();

        List<WorkItemData> GetWorkItems();

        WorkItemData GetWorkItem(string id);

        WorkItemData GetWorkItem(int id);

        List<WorkItemData> GetWorkItems(string WIQLQuery);

        List<WorkItemData> GetWorkItems(IWorkItemQueryBuilder queryBuilder);

        WorkItemData PersistWorkItem(WorkItemData workItem);

        WorkItemData GetRevision(WorkItemData workItem, int revision);

        WorkItemData FindReflectedWorkItem(WorkItemData workItem, bool cache);

        WorkItemData FindReflectedWorkItemByTitle(string title);

        WorkItemData FindReflectedWorkItemByMigrationRef(ReflectedWorkItemId refId);

        WorkItemData FindReflectedWorkItemByReflectedWorkItemId(string refId);

        WorkItemData FindReflectedWorkItemByReflectedWorkItemId(ReflectedWorkItemId refId, bool cache);

        WorkItemData FindReflectedWorkItemByReflectedWorkItemId(WorkItemData refWi);

        ReflectedWorkItemId CreateReflectedWorkItemId(WorkItemData workItem);

        ReflectedWorkItemId GetReflectedWorkItemId(WorkItemData workItem);
    }
}