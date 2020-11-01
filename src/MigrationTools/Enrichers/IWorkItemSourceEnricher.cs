using MigrationTools.DataContracts;

namespace MigrationTools.Enrichers
{
    public interface IWorkItemSourceEnricher : IWorkItemEnricher
    {
        int EnrichToWorkItem(WorkItemData2 workItem);
    }
}