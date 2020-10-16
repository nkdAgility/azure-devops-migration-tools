using System.Collections.Generic;
using MigrationTools.Clients;
using MigrationTools.DataContracts;

namespace MigrationTools.EndPoints
{
    public interface IWorkItemSourceEndPoint : IWorkItemEndPoint
    {
        void Filter(IEnumerable<WorkItemData> targetWorkItems);
    }

    public interface IWorkItemTargetEndPoint : IWorkItemEndPoint
    {
        void PersistWorkItem(WorkItemData sourceWorkItem);
    }

    public interface IWorkItemEndPoint : IEndpoint
    {
        IEnumerable<WorkItemData> WorkItems { get; }

        void Configure(IWorkItemQuery query);
    }
}