using System.Collections.Generic;
using MigrationTools.DataContracts;
using MigrationTools.Options;

namespace MigrationTools.Endpoints
{
    public interface IWorkItemSourceEndpoint : ISourceEndPoint, IWorkItemEndpoint
    {
        void Filter(IEnumerable<WorkItemData> targetWorkItems);

        IEnumerable<WorkItemData> GetWorkItems();

        IEnumerable<WorkItemData> GetWorkItems(QueryOptions query);
    }

    public interface IWorkItemTargetEndpoint : ITargetEndPoint, IWorkItemEndpoint
    {
        void PersistWorkItem(WorkItemData sourceWorkItem);
    }

    public interface IWorkItemEndpoint : IEndpoint
    {
    }
}