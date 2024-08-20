using System;
using Newtonsoft.Json;

namespace MigrationTools.EndpointEnrichers
{
    public abstract class EndpointEnricherOptions : IEndpointEnricherOptions
    {
        public virtual string ConfigurationSectionPath => $"MigrationTools:EndpointEnricherDefaults:{ConfigurationOptionFor}";
        public string ConfigurationCollectionPath => $"MigrationTools:Endpoints:*:Enrichers:*:{ConfigurationOptionFor}";
        public virtual string ConfigurationObjectName => $"EndpointEnricherType";
        public virtual string ConfigurationOptionFor => $"{GetType().Name.Replace("Options", "")}";

        
        public bool Enabled { get; set; }

        public abstract Type ToConfigure { get; }
        public string RefName { get; set; }

        public abstract void SetDefaults();
    }
}