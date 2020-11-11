using System;
using Microsoft.Extensions.Logging;
using MigrationTools.EndpointEnrichers;

namespace MigrationTools.Endpoints
{
    public class TfsTeamSettingsEndpoint : TfsEndpoint
    {
        public TfsTeamSettingsEndpoint(EndpointEnricherContainer endpointEnrichers, IServiceProvider services, ITelemetryLogger telemetry, ILogger<Endpoint> logger) : base(endpointEnrichers, services, telemetry, logger)
        {
        }

        public override int Count => throw new NotImplementedException();
    }
}