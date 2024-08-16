using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MigrationTools._EngineV1.Enrichers;
using MigrationTools.DataContracts;
using MigrationTools.Enrichers;
using MigrationTools.Processors;

namespace MigrationTools.Clients.AzureDevops.Rest.Enrichers
{
    public class EmbededImagesRepairEnricher : EmbededImagesRepairEnricherBase
    {
        public EmbededImagesRepairEnricher(IServiceProvider services, ILogger<EmbededImagesRepairEnricher> logger, ITelemetryLogger telemetryLogger) : base(services, logger, telemetryLogger)
        {
            Engine = services.GetRequiredService<IMigrationEngine>();
        }

        public IMigrationEngine Engine { get; private set; }

        [Obsolete]
        public override int Enrich(WorkItemData sourceWorkItem, WorkItemData targetWorkItem)
        {
            throw new NotImplementedException();
        }

        protected override void EntryForProcessorType(IProcessor processor)
        {
            throw new NotImplementedException();
        }

        protected override void FixEmbededImages(WorkItemData wi, string oldTfsurl, string newTfsurl, string sourcePersonalAccessToken = "")
        {
            throw new NotImplementedException();
        }

        protected override void RefreshForProcessorType(IProcessor processor)
        {
            throw new NotImplementedException();
        }
    }
}