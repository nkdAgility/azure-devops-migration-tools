using MigrationTools.DataContracts;

namespace MigrationTools.Enrichers
{
   public interface IEmbededImagesRepairEnricher
    {
        void FixEmbededImages(WorkItemData wi, string oldTfsurl, string newTfsurl, string sourcePersonalAccessToken = "");
    }
}