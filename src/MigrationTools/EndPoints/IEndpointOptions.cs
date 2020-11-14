using System.Collections.Generic;
using MigrationTools.EndpointEnrichers;
using MigrationTools.Options;
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

    //[JsonConverter(typeof(OptionsJsonConvertor<IEndpointOptions>))]
    public interface IEndpointOptions : IOptions
    {
        [JsonConverter(typeof(StringEnumConverter)), JsonProperty(Order = -200)]
        public EndpointDirection Direction { get; set; }

        public List<IEndpointEnricherOptions> EndpointEnrichers { get; set; }
    }
}