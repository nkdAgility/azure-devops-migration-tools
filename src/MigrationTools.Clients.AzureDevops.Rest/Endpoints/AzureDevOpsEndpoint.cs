using System;
using Microsoft.Extensions.Logging;
using MigrationTools.EndpointEnrichers;

namespace MigrationTools.Endpoints
{
    public class AzureDevOpsEndpoint : Endpoint<AzureDevOpsEndpointOptions>
    {
        public string AccessToken => Options.AccessToken;
        public string Organisation => Options.Organisation;
        public string Project => Options.Project;

        public override int Count => 0;

        public AzureDevOpsEndpoint(EndpointEnricherContainer endpointEnrichers, ITelemetryLogger telemetry, ILogger<AzureDevOpsEndpoint> logger)
            : base(endpointEnrichers, telemetry, logger)
        {
        }

        public override void Configure(AzureDevOpsEndpointOptions options)
        {
            base.Configure(options);
            Log.LogDebug("AzureDevOpsEndpoint::Configure");
            if (string.IsNullOrEmpty(Options.Organisation))
            {
                throw new ArgumentNullException(nameof(Options.Organisation));
            }
            if (string.IsNullOrEmpty(Options.Project))
            {
                throw new ArgumentNullException(nameof(Options.Project));
            }
            if (string.IsNullOrEmpty(Options.AccessToken))
            {
                throw new ArgumentNullException(nameof(Options.AccessToken));
            }
        }
    }
}