using MigrationTools.DataContracts;

namespace MigrationTools.Tools.Infra
{
    public interface IAttachmentMigrationEnricher
    {
        void ProcessAttachemnts(WorkItemData source, WorkItemData target, bool save = true);

        void CleanUpAfterSave();
    }
}