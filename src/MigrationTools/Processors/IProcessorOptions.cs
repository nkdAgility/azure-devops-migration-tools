using System.Collections.Generic;
using MigrationTools._EngineV1.Configuration;
using MigrationTools.Endpoints;
using MigrationTools.Enrichers;
using MigrationTools.Options;
using Newtonsoft.Json;

namespace MigrationTools.Processors
{
    [JsonConverter(typeof(OptionsJsonConvertor<IProcessorOptions>))]
    public interface IProcessorOptions : IProcessorConfig, IOptions
    {
        List<IEndpointOptions> Endpoints { get; set; }

        List<IProcessorEnricherOptions> ProcessorEnrichers { get; set; }

        IProcessorOptions GetDefault();
    }
}