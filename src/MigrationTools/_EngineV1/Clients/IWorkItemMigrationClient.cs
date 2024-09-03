using System.Collections.Generic;
using MigrationTools._EngineV1.Configuration;
using MigrationTools._EngineV1.DataContracts;
using MigrationTools.DataContracts;
using MigrationTools.Endpoints;
using MigrationTools.Endpoints.Infrastructure;

namespace MigrationTools.Clients
{
    public interface IWorkItemMigrationClient
    {
        IEndpointOptions Options { get; }
        ProjectData Project { get; }

        ProjectData GetProject();

        List<WorkItemData> GetWorkItems();

        WorkItemData GetWorkItem(string id, bool stopOnError = true);

        WorkItemData GetWorkItem(int id, bool stopOnError = true);

        List<int> GetWorkItemIds(string WIQLQuery);

        List<WorkItemData> GetWorkItems(string WIQLQuery);

        List<WorkItemData> GetWorkItems(IWorkItemQueryBuilder queryBuilder);

        WorkItemData PersistWorkItem(WorkItemData workItem);

        WorkItemData GetRevision(WorkItemData workItem, int revision);

        WorkItemData FindReflectedWorkItem(WorkItemData workItem, bool cache);

        WorkItemData FindReflectedWorkItemByReflectedWorkItemId(string refId);

        WorkItemData FindReflectedWorkItemByReflectedWorkItemId(WorkItemData refWi);

        ReflectedWorkItemId CreateReflectedWorkItemId(WorkItemData workItem);

        ReflectedWorkItemId GetReflectedWorkItemId(WorkItemData workItem);
    }
}