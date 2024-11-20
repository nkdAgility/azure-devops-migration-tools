using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MigrationTools.EndpointEnrichers;
using MigrationTools.Endpoints;
using MigrationTools.Endpoints.Infrastructure;

namespace MigrationTools.Endpoints.Shadows
{

    public class FakeEndpoint : Endpoint<FakeEndpointOptions>
    {
        new public FakeEndpointOptions Options => (FakeEndpointOptions)base.Options;

        public FakeEndpoint(IOptions<FakeEndpointOptions> options, EndpointEnricherContainer endpointEnrichers, IServiceProvider serviceProvider, ITelemetryLogger telemetry, ILogger<FakeEndpoint> logger) : base(options, endpointEnrichers, serviceProvider, telemetry, logger)
        {
        }

        [Obsolete("Dont know what this is for")]
        public override int Count => 0;
    }

    public class FakeEndpointOptions : EndpointOptions
    {
        public string Token { get; set; }
    }
}
