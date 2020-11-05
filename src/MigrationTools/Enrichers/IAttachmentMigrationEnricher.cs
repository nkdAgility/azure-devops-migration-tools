using MigrationTools._EngineV1.DataContracts;

namespace MigrationTools.Enrichers
{
    public interface IAttachmentMigrationEnricher
    {
        void ProcessAttachemnts(WorkItemData source, WorkItemData target, bool save = true);

        void CleanUpAfterSave();
    }
}