using System.Collections.Generic;
using MigrationTools.Enrichers;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MigrationTools.Endpoints
{
    public class EndpointOptions : IEndpointOptions
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public EndpointDirection Direction { get; set; }

        [JsonIgnoreAttribute]
        public virtual string Endpoint { get { return "Should override!"; } }

        public List<IEndpointEnricher> EndpointEnrichers { get; set; }
    }
}