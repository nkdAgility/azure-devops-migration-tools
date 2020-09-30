using MigrationTools.DataContracts;

namespace MigrationTools.Engine.Enrichers
{
   public interface IEmbededImagesRepairEnricher
    {
        void FixEmbededImages(WorkItemData wi, string oldTfsurl, string newTfsurl, string sourcePersonalAccessToken = "");
    }
}