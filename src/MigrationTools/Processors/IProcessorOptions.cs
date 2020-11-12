using System.Collections.Generic;
using MigrationTools._EngineV1.Configuration;
using MigrationTools.Endpoints;
using MigrationTools.Enrichers;
using MigrationTools.Options;
using Newtonsoft.Json;

namespace MigrationTools.Processors
{
    public interface IProcessorOptions : IProcessorConfig, IOptions
    {
        [JsonConverter(typeof(IOptionsJsonConvertor))]
        List<IEndpointOptions> Endpoints { get; set; }

        [JsonConverter(typeof(IOptionsJsonConvertor))]
        List<IProcessorEnricherOptions> ProcessorEnrichers { get; set; }

        IProcessorOptions GetDefault();
    }
}