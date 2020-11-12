using System.Collections.Generic;
using MigrationTools._EngineV1.Configuration;
using MigrationTools.Endpoints;
using MigrationTools.Enrichers;
using MigrationTools.Options;

namespace MigrationTools.Processors
{
    public interface IProcessorOptions : IProcessorConfig, IOptions
    {
        List<IEndpointOptions> Endpoints { get; set; }
        List<IProcessorEnricherOptions> ProcessorEnrichers { get; set; }

        IProcessorOptions GetDefault();
    }
}