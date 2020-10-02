using MigrationTools.DataContracts;

namespace MigrationTools.Enrichers
{
    public interface IWorkItemEnricher
    {
        IMigrationEngine Engine { get; }

        void Configure(bool save = true, bool filterWorkItemsThatAlreadyExistInTarget = true);
        void Enritch(WorkItemData sourceWorkItem, WorkItemData targetWorkItem);
    }
}