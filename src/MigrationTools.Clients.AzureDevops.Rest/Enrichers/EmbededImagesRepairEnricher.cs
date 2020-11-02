using System;
using Microsoft.Extensions.Logging;
using MigrationTools.DataContracts;
using MigrationTools.Enrichers;

namespace MigrationTools.Clients.AzureDevops.Rest.Enrichers
{
    public class EmbededImagesRepairEnricher : EmbededImagesRepairEnricherBase
    {
        public EmbededImagesRepairEnricher(IMigrationEngine engine, ILogger<EmbededImagesRepairEnricher> logger) : base(engine, logger)
        {
        }

        public override void Configure(bool save = true, bool filter = true)
        {
            throw new NotImplementedException();
        }

        public override void Configure(IProcessorEnricherOptions options)
        {
            throw new NotImplementedException();
        }

        public override int Enrich(WorkItemData sourceWorkItem, WorkItemData targetWorkItem)
        {
            throw new NotImplementedException();
        }

        protected override void FixEmbededImages(WorkItemData wi, string oldTfsurl, string newTfsurl, string sourcePersonalAccessToken = "")
        {
            throw new NotImplementedException();
        }
    }
}