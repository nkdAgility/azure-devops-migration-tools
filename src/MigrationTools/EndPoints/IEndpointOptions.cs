using System;
using System.Collections.Generic;
using MigrationTools.EndpointEnrichers;
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

        [JsonIgnoreAttribute]
        public Type ToConfigure { get; }

        public List<IEndpointEnricherOptions> Enrichers { get; set; }

        void SetDefaults();
    }
}