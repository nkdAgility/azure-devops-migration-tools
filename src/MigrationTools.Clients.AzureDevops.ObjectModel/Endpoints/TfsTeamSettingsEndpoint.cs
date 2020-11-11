using System;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.ProcessConfiguration.Client;
using MigrationTools.EndpointEnrichers;

namespace MigrationTools.Endpoints
{
    public class TfsTeamSettingsEndpoint : TfsEndpoint, ITfsTeamSettingsEndpointOptions, ISourceEndPoint, ITargetEndPoint
    {
        public TfsTeamSettingsEndpoint(EndpointEnricherContainer endpointEnrichers, IServiceProvider services, ITelemetryLogger telemetry, ILogger<Endpoint> logger) : base(endpointEnrichers, services, telemetry, logger)
        {
        }

        public TfsTeamService TfsTeamService { get { return TfsCollection.GetService<TfsTeamService>(); } }

        public TeamSettingsConfigurationService TfsTeamSettingsService { get { return TfsCollection.GetService<TeamSettingsConfigurationService>(); } }
    }
}