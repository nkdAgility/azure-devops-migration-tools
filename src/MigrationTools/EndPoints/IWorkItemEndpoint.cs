using System.Collections.Generic;
using MigrationTools.DataContracts;
using MigrationTools.Options;

namespace MigrationTools.Endpoints
{
    public interface IWorkItemSourceEndPoint : ISourceEndPoint, IWorkItemEndPoint
    {
        void Filter(IEnumerable<WorkItemData> targetWorkItems);

        IEnumerable<WorkItemData> GetWorkItems();

        IEnumerable<WorkItemData> GetWorkItems(QueryOptions query);
    }

    public interface IWorkItemTargetEndPoint : ITargetEndPoint, IWorkItemEndPoint
    {
        void PersistWorkItem(WorkItemData sourceWorkItem);
    }

    public interface IWorkItemEndPoint : IEndpoint
    {
    }
}