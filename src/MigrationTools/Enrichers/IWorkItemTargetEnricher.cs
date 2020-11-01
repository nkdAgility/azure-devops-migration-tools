using MigrationTools.DataContracts;

namespace MigrationTools.Enrichers
{
    public interface IWorkItemTargetEnricher : IWorkItemEnricher
    {
        int PersistFromWorkItem(WorkItemData2 workItem);
    }
}