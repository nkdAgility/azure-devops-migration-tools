using Newtonsoft.Json;

namespace MigrationTools.Enrichers
{
    [JsonConverter(typeof(ProcessorEnricherOptionsJsonConvertor))]
    public interface IProcessorEnricherOptions : IEnricherOptions
    {
    }
}