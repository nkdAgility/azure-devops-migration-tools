using System.Collections.Generic;
using MigrationTools.EndpointEnrichers;
using MigrationTools.Options;

namespace MigrationTools.Endpoints.Infrastructure
{
    public interface IEndpointOptions : IOptions
    {
        //void SetDefaults();
        string Name { get; set; }
        public List<IEndpointEnricherOptions> EndpointEnrichers { get; set; }
    }

}