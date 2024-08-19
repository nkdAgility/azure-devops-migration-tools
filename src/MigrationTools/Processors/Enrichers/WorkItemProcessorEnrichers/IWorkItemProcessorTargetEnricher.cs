using MigrationTools.DataContracts;

namespace MigrationTools.Enrichers
{
    public interface IWorkItemProcessorTargetEnricher : IWorkItemProcessorEnricher
    {
        int PersistFromWorkItem(WorkItemData workItem);
    }
}