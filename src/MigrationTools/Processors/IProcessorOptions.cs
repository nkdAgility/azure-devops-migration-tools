using System.Collections.Generic;
using MigrationTools._EngineV1.Configuration;
using MigrationTools.Endpoints;
using MigrationTools.Enrichers;

namespace MigrationTools.Processors
{
    public interface IProcessorOptions : IProcessorConfig
    {
        List<IEndpointOptions> Endpoints { get; set; }
        List<ProcessorEnricherOptions> Enrichers { get; set; }

        IProcessorOptions GetDefault();
    }
}