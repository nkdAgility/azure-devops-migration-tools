using System.Collections.Generic;
using MigrationTools.EndpointEnrichers;
using MigrationTools.Options;

namespace MigrationTools.Endpoints
{
    public interface IEndpointOptions : IOptions
    {
        //void SetDefaults();
        public List<IEndpointEnricherOptions> EndpointEnrichers { get; set; }
    }

}