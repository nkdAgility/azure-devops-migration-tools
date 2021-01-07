using System;
using Microsoft.Extensions.Logging;
using MigrationTools.EndpointEnrichers;

namespace MigrationTools.Endpoints
{
    public class AzureDevOpsEndpoint : Endpoint
    {
        private AzureDevOpsEndpointOptions _Options;

        public string AccessToken => _Options.AccessToken;
        public string Organisation => _Options.Organisation;
        public string Project => _Options.Project;
        public string ReflectedWorkItemIdField => _Options.ReflectedWorkItemIdField;
        public AuthenticationMode AuthenticationMode => _Options.AuthenticationMode;

        public override int Count => 0;

        public AzureDevOpsEndpoint(EndpointEnricherContainer endpointEnrichers, ITelemetryLogger telemetry, ILogger<AzureDevOpsEndpoint> logger)
            : base(endpointEnrichers, telemetry, logger)
        {
        }

        public override void Configure(IEndpointOptions options)
        {
            base.Configure(options);
            Log.LogDebug("AzureDevOpsEndpoint::Configure");
            _Options = (AzureDevOpsEndpointOptions)options;
            if (string.IsNullOrEmpty(_Options.Organisation))
            {
                throw new ArgumentNullException(nameof(_Options.Organisation));
            }
            if (string.IsNullOrEmpty(_Options.Project))
            {
                throw new ArgumentNullException(nameof(_Options.Project));
            }
            if (string.IsNullOrEmpty(_Options.AccessToken))
            {
                throw new ArgumentNullException(nameof(_Options.AccessToken));
            }
        }
    }
}