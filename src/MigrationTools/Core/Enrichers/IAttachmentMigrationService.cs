using MigrationTools.Core.DataContracts;

namespace MigrationTools.Core.Engine.Enrichers
{
    public interface IAttachmentMigrationEnricher
    {
        void ProcessAttachemnts(WorkItemData source, WorkItemData target, bool save = true);
        void CleanUpAfterSave();
    }
}