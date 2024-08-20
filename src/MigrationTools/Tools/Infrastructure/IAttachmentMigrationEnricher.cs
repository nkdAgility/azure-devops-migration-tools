using MigrationTools.DataContracts;

namespace MigrationTools.Tools.Infrastructure
{
    public interface IAttachmentMigrationEnricher
    {
        void ProcessAttachemnts(WorkItemData source, WorkItemData target, bool save = true);

        void CleanUpAfterSave();
    }
}