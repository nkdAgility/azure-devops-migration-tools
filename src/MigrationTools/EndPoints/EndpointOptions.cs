using System;
using System.Collections.Generic;
using MigrationTools.EndpointEnrichers;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MigrationTools.Endpoints
{
    public abstract class EndpointOptions : IEndpointOptions
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public EndpointDirection Direction { get; set; }

        [JsonIgnoreAttribute]
        public abstract Type ToConfigure { get; }

        public List<IEndpointEnricherOptions> EndpointEnrichers { get; set; }

        public virtual void SetDefaults()
        {
            Direction = EndpointDirection.Source;
        }
    }
}