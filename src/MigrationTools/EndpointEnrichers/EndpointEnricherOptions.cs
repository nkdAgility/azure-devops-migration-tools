using System;
using Newtonsoft.Json;

namespace MigrationTools.EndpointEnrichers
{
    public abstract class EndpointEnricherOptions : IEndpointEnricherOptions
    {
        [JsonIgnore]
        public virtual string ConfigurationSectionName => $"MigrationTools:EndpointEnricherDefaults:{OptionFor}";
        [JsonIgnore]
        public virtual string OptionFor => $"{GetType().Name.Replace("Options", "")}";

        [JsonProperty(Order = -2)]
        public bool Enabled { get; set; }

        public abstract Type ToConfigure { get; }
        public string RefName { get; set; }

        public abstract void SetDefaults();
    }
}