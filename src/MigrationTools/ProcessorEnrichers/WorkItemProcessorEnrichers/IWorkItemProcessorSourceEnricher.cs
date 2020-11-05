using MigrationTools.DataContracts;

namespace MigrationTools.Enrichers
{
    public interface IWorkItemProcessorSourceEnricher : IWorkItemProcessorEnricher
    {
        int EnrichToWorkItem(WorkItemData workItem);
    }
}