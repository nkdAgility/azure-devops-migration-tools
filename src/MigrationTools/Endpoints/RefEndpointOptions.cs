using System;
using System.Collections.Generic;
using MigrationTools.EndpointEnrichers;

namespace MigrationTools.Endpoints
{
    public class RefEndpointOptions : IEndpointOptions
    {
        public virtual string ConfigurationSectionName => $"MigrationTools:RefEndpointDefaults:{OptionFor}";
        public string OptionFor => $"{GetType().Name.Replace("Options", "")}";

        public List<IEndpointEnricherOptions> EndpointEnrichers { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public string RefName { get; set; }

       

        public void SetDefaults()
        {
            throw new NotImplementedException();
        }
    }
}