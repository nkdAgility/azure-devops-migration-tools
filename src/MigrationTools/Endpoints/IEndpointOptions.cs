using System.Collections.Generic;
using MigrationTools.EndpointEnrichers;

namespace MigrationTools.Endpoints
{
    public interface IEndpointOptions
    {
        //void SetDefaults();
        public List<IEndpointEnricherOptions> EndpointEnrichers { get; set; }
    }
}