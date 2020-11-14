using System;
using System.Collections.Generic;
using MigrationTools.EndpointEnrichers;
using Newtonsoft.Json;

namespace MigrationTools.Endpoints
{
    public abstract class EndpointOptions : IEndpointOptions
    {
        [JsonIgnoreAttribute]
        public abstract Type ToConfigure { get; }

        public List<IEndpointEnricherOptions> EndpointEnrichers { get; set; }

        public virtual void SetDefaults()
        {
        }
    }
}