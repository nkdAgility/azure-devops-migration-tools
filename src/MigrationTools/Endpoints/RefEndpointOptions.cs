using System;
using System.Collections.Generic;
using MigrationTools.EndpointEnrichers;
using Newtonsoft.Json;

namespace MigrationTools.Endpoints
{
    public class RefEndpointOptions : IEndpointOptions
    {
        public virtual string ConfigurationSectionPath => $"MigrationTools:RefEndpointDefaults:{ConfigurationOptionFor}";
        public string ConfigurationCollectionPath => $"MigrationTools:Endpoints:*:{ConfigurationOptionFor}";
        public string ConfigurationCollectionObjectName => $"EndpointType";
        public string ConfigurationOptionFor => $"{GetType().Name.Replace("Options", "")}";

        public List<IEndpointEnricherOptions> EndpointEnrichers { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string RefName { get; set; }

        public bool Enabled { get; set; }
    }
}