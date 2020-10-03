using MigrationTools.DataContracts;

namespace MigrationTools.Enrichers
{
    public interface IWorkItemEnricher
    {
        void Configure(bool save = true, bool filter = true);
        int Enrich(WorkItemData sourceWorkItem, WorkItemData targetWorkItem);
    }
}