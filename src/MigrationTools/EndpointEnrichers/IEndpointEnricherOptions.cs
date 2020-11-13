using MigrationTools.Enrichers;
using Newtonsoft.Json;

namespace MigrationTools.EndpointEnrichers
{
    [JsonConverter(typeof(EndpointEnricherOptionsJsonConvertor))]
    public interface IEndpointEnricherOptions : IEnricherOptions
    {
    }
}