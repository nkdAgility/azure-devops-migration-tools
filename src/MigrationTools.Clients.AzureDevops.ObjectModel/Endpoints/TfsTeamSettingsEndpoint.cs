using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.ProcessConfiguration.Client;
using Microsoft.TeamFoundation.Work.WebApi;
using MigrationTools.EndpointEnrichers;

namespace MigrationTools.Endpoints
{
    public class TfsTeamSettingsEndpoint : GenericTfsEndpoint<TfsTeamSettingsEndpointOptions>, ISourceEndPoint, ITargetEndPoint
    {
        public TfsTeamSettingsEndpoint(IOptions<TfsTeamSettingsEndpointOptions> options, EndpointEnricherContainer endpointEnrichers, IServiceProvider serviceProvider, ITelemetryLogger telemetry, ILogger<Endpoint<TfsTeamSettingsEndpointOptions>> logger) : base(options, endpointEnrichers, serviceProvider, telemetry, logger)
        {
        }

        public TfsTeamService TfsTeamService { get { return TfsCollection.GetService<TfsTeamService>(); } }

        public TeamSettingsConfigurationService TfsTeamSettingsService { get { return TfsCollection.GetService<TeamSettingsConfigurationService>(); } }

        public WorkHttpClient WorkHttpClient { get { return TfsCollection.GetClient<WorkHttpClient>(); } }
    }
}