using MigrationTools.Options;
using Newtonsoft.Json;

namespace MigrationTools.Enrichers
{
    [JsonConverter(typeof(OptionsJsonConvertor))]
    public interface IProcessorEnricherOptions : IEnricherOptions
    {
    }
}