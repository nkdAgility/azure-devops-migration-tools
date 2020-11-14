using System;
using Newtonsoft.Json;

namespace MigrationTools.EndpointEnrichers
{
    [JsonObjectAttribute(Description = "moo")]
    public abstract class EndpointEnricherOptions : IEndpointEnricherOptions
    {
        public bool Enabled { get; set; }

        public abstract Type ToConfigure { get; }

        public abstract void SetDefaults();
    }
}