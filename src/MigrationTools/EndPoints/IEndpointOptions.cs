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

    [JsonConverter(typeof(OptionsJsonConvertor))]
    public interface IEndpointOptions : IOptions
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public EndpointDirection Direction { get; set; }

        public List<IEndpointEnricherOptions> Enrichers { get; set; }
    }
}