using MigrationTools.Core.DataContracts;

namespace MigrationTools.Core.Engine.Enrichers
{
   public interface IEmbededImagesRepairEnricher
    {
        void FixEmbededImages(WorkItemData wi, string oldTfsurl, string newTfsurl, string sourcePersonalAccessToken = "");
    }
}