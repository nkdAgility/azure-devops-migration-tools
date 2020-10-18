using System.Collections.Generic;
using MigrationTools.Clients;
using MigrationTools.DataContracts;
using MigrationTools.Enrichers;

namespace MigrationTools.EndPoints
{
    public interface IWorkItemSourceEndPoint : IWorkItemEndPoint
    {
        IEnumerable<IWorkItemSourceEnricher> SourceEnrichers { get; }

        void Filter(IEnumerable<WorkItemData> targetWorkItems);

        IEnumerable<WorkItemData> GetWorkItems();
    }

    public interface IWorkItemTargetEndPoint : IWorkItemEndPoint
    {
        IEnumerable<IWorkItemTargetEnricher> TargetEnrichers { get; }

        void PersistWorkItem(WorkItemData sourceWorkItem);
    }

    public interface IWorkItemEndPoint : IEndpoint
    {
        void Configure(IWorkItemQuery query, List<IWorkItemEnricher> enrichers);
    }
}