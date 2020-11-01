using System.Collections.Generic;
using MigrationTools.Enrichers;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MigrationTools.Endpoints
{
    public abstract class EndpointOptions : IEndpointOptions
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public EndpointDirection Direction { get; set; }

        [JsonIgnoreAttribute]
        public abstract string Endpoint { get; }

        public List<IEndpointEnricher> Enrichers { get; set; }

        public abstract void SetDefaults();
    }
}