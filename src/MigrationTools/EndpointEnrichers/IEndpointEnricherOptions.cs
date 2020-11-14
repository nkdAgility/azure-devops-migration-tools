using MigrationTools.Enrichers;
using MigrationTools.Options;
using Newtonsoft.Json;

namespace MigrationTools.EndpointEnrichers
{
    [JsonConverter(typeof(OptionsJsonConvertor<IEndpointEnricherOptions>))]
    public interface IEndpointEnricherOptions : IEnricherOptions
    {
    }
}