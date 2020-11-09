using System.Collections.Generic;
using MigrationTools.DataContracts;
using MigrationTools.Enrichers;
using MigrationTools.Options;

namespace MigrationTools.Endpoints
{
    public interface IWorkItemSourceEndPoint : IWorkItemEndPoint
    {
        IEnumerable<IWorkItemProcessorSourceEnricher> SourceEnrichers { get; }

        void Filter(IEnumerable<WorkItemData> targetWorkItems);

        IEnumerable<WorkItemData> GetWorkItems();

        IEnumerable<WorkItemData> GetWorkItems(QueryOptions query);
    }

    public interface IWorkItemTargetEndPoint : IWorkItemEndPoint
    {
        IEnumerable<IWorkItemProcessorTargetEnricher> TargetEnrichers { get; }

        void PersistWorkItem(WorkItemData sourceWorkItem);
    }

    public interface IWorkItemEndPoint : IEndpoint
    {
    }
}