using System.Collections.Generic;
using MigrationTools.EndpointEnrichers;
using MigrationTools.Options;
using Newtonsoft.Json;

namespace MigrationTools.Endpoints.Infrastructure
{
    public abstract class EndpointOptions : IEndpointOptions
    {
        [JsonIgnore]
        public string OptionFor => $"{GetType().Name.Replace("Options", "")}";

        [JsonIgnore]
        public ConfigurationMetadata ConfigurationMetadata => new ConfigurationMetadata
        {
            IsCollection = false,
            IsKeyed = true,
            PathToInstance = $"MigrationTools:Endpoints:#KEY#:{OptionFor}",
            ObjectName = $"EndpointType",
            OptionFor = OptionFor,
            PathToDefault = $"MigrationTools:EndpointDefaults:{OptionFor}",
            PathToSample = $"MigrationTools:EndpointSamples:{OptionFor}"
        };

        [JsonIgnore]
        public string Name { get; set; }
        public List<IEndpointEnricherOptions> EndpointEnrichers { get; set; }
        public bool Enabled { get; set; }

        //public virtual void SetDefaults()
        //{
        //}
    }
}