using System.Collections.Generic;
using MigrationTools.Configuration;
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