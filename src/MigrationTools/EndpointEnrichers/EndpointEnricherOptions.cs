using System;
using Newtonsoft.Json;

namespace MigrationTools.EndpointEnrichers
{
    public abstract class EndpointEnricherOptions : IEndpointEnricherOptions
    {
        [JsonProperty(Order = -2)]
        public bool Enabled { get; set; }

        public abstract Type ToConfigure { get; }

        public abstract void SetDefaults();
    }
}