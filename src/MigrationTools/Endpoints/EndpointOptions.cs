using System.Collections.Generic;
using MigrationTools.EndpointEnrichers;
using Newtonsoft.Json;

namespace MigrationTools.Endpoints
{
    public abstract class EndpointOptions : IEndpointOptions
    {
        public virtual string ConfigurationSectionPath => $"MigrationTools:EndpointDefaults:{ConfigurationOptionFor}";
        public string ConfigurationCollectionPath => $"MigrationTools:Endpoints:*:{ConfigurationOptionFor}";
        public virtual string ConfigurationCollectionObjectName => $"EndpointType";

        public string ConfigurationOptionFor => $"{GetType().Name.Replace("Options", "")}";

        public string Name { get; set; }
        public List<IEndpointEnricherOptions> EndpointEnrichers { get; set; }
        public bool Enabled { get; set; }

        //public virtual void SetDefaults()
        //{
        //}
    }
}