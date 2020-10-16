using System.Collections.Generic;
using MigrationTools.Clients;
using MigrationTools.DataContracts;

namespace MigrationTools.EndPoints
{
    public interface IWorkItemSourceEndPoint : IEndpoint
    {
        IEnumerable<WorkItemData> WorkItems { get; }

        void Configure(IWorkItemQuery query);
    }

    public interface IWorkItemTargetEndPoint : IEndpoint
    {
        void Configure(IWorkItemQuery query);

        void Filter(IEnumerable<WorkItemData> workItems);

        void PersistWorkItem(WorkItemData source);
    }
}