using System.Collections.ObjectModel;
using MigrationTools.Enrichers;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MigrationTools.Endpoints
{
    public enum EndpointDirection
    {
        NotSet = 0,
        Source = 1,
        Target = 2
    }

    public interface IEndpointOptions
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public EndpointDirection Direction { get; set; }

        public Collection<IEndpointEnricher> EndpointEnrichers { get; set; }
    }
}