using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MigrationTools.Configuration
{
    public enum EndpointDirection
    {
        Source,
        Target
    }

    public interface IEndpointConfig
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public EndpointDirection Direction { get; set; }
    }
}