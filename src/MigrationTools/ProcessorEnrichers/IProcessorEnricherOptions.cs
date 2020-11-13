using MigrationTools.Options;
using Newtonsoft.Json;

namespace MigrationTools.Enrichers
{
    [JsonConverter(typeof(OptionsJsonConvertor<IProcessorEnricherOptions>))]
    public interface IProcessorEnricherOptions : IEnricherOptions
    {
    }
}