using System;
using MigrationTools.Options;
using Newtonsoft.Json;

namespace MigrationTools.EndpointEnrichers
{
    public abstract class EndpointEnricherOptions : IEndpointEnricherOptions
    {
        [JsonIgnore]
        public string OptionFor => $"{GetType().Name.Replace("Options", "")}";

        [JsonIgnore]
        public ConfigurationMetadata ConfigurationMetadata => new ConfigurationMetadata
        {
            IsCollection = true,
            PathToInstance = null,
            ObjectName = $"ProcessorType",
            OptionFor = OptionFor,
            PathToDefault = $"MigrationTools:Endpoints:EnricherDefaults:{OptionFor}",
            PathToSample = $"MigrationTools:Endpoints:EnricherSamples:{OptionFor}"
        };
    
        public bool Enabled { get; set; }

        public abstract Type ToConfigure { get; }
        public string RefName { get; set; }

        

        public abstract void SetDefaults();
    }
}