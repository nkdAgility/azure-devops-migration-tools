using System.Collections.Generic;
using MigrationTools.DataContracts;
using MigrationTools.Enrichers;

namespace MigrationTools.Endpoints
{
    public interface IWorkItemSourceEndPoint : IWorkItemEndPoint
    {
        IEnumerable<IWorkItemProcessorSourceEnricher> SourceEnrichers { get; }

        void Filter(IEnumerable<WorkItemData2> targetWorkItems);

        IEnumerable<WorkItemData2> GetWorkItems();

        IEnumerable<WorkItemData2> GetWorkItems(IWorkItemQuery query);
    }

    public interface IWorkItemTargetEndPoint : IWorkItemEndPoint
    {
        IEnumerable<IWorkItemProcessorTargetEnricher> TargetEnrichers { get; }

        void PersistWorkItem(WorkItemData2 sourceWorkItem);
    }

    public interface IWorkItemEndPoint : IEndpoint
    {
    }
}