using System;
using Newtonsoft.Json;

namespace MigrationTools.EndpointEnrichers
{
    public abstract class EndpointEnricherOptions : IEndpointEnricherOptions
    {
        public string ConfigurationSectionPath => $"MigrationTools:EndpointEnricherDefaults:{ConfigurationOptionFor}";
        public string ConfigurationCollectionPath => $"MigrationTools:Endpoints:*:Enrichers:*:{ConfigurationOptionFor}";
        public string ConfigurationObjectName => $"EndpointEnricherType";
        public string ConfigurationOptionFor => $"{GetType().Name.Replace("Options", "")}";
        public string ConfigurationSamplePath => $"MigrationTools:EndpointEnricherSamples:{ConfigurationOptionFor}";

        public bool Enabled { get; set; }

        public abstract Type ToConfigure { get; }
        public string RefName { get; set; }

        

        public abstract void SetDefaults();
    }
}