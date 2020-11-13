using MigrationTools.Enrichers;
using MigrationTools.Options;
using Newtonsoft.Json;

namespace MigrationTools.EndpointEnrichers
{
    [JsonConverter(typeof(OptionsJsonConvertor))]
    public interface IEndpointEnricherOptions : IEnricherOptions
    {
    }
}