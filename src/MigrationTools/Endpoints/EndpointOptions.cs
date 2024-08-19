using System.Collections.Generic;
using MigrationTools.EndpointEnrichers;

namespace MigrationTools.Endpoints
{
    public abstract class EndpointOptions : IEndpointOptions
    {
        public virtual string ConfigurationSectionName => $"MigrationTools:EndpointDefaults:{OptionFor}";
        public string OptionFor => $"{GetType().Name.Replace("Options", "")}";

        public string Name { get; set; }
        public List<IEndpointEnricherOptions> EndpointEnrichers { get; set; }

        //public virtual void SetDefaults()
        //{
        //}
    }
}