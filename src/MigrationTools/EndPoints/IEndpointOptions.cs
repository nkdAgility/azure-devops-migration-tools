using System.Collections.Generic;
using MigrationTools.EndpointEnrichers;
using MigrationTools.Options;

namespace MigrationTools.Endpoints
{
    //[JsonConverter(typeof(OptionsJsonConvertor<IEndpointOptions>))]
    public interface IEndpointOptions : IOptions
    {
        public List<IEndpointEnricherOptions> EndpointEnrichers { get; set; }
    }
}