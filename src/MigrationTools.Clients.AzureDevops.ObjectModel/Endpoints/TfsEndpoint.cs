using System;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.Client;
using Microsoft.VisualStudio.Services.Common;
using MigrationTools.EndpointEnrichers;

namespace MigrationTools.Endpoints
{
    public abstract class TfsEndpoint : Endpoint
    {
        private TfsTeamProjectCollection _Collection;
        private TfsEndpointOptions _Options;

        protected TfsTeamProjectCollection Collection
        {
            get
            {
                return GetTfsCollection();
            }
        }

        protected TfsEndpoint(EndpointEnricherContainer endpointEnrichers, IServiceProvider services, ITelemetryLogger telemetry, ILogger<Endpoint> logger) : base(endpointEnrichers, services, telemetry, logger)
        {
        }

        public override void Configure(IEndpointOptions options)
        {
            base.Configure(options);
            Log.LogDebug("TfsEndpoint::Configure");
            _Options = (TfsEndpointOptions)options;
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

        private TfsTeamProjectCollection GetTfsCollection()
        {
            if (_Collection is null)
            {
                var startTime = DateTime.UtcNow;
                var timer = System.Diagnostics.Stopwatch.StartNew();
                VssCredentials vssCredentials;
                try
                {
                    if (!string.IsNullOrEmpty(_Options.AccessToken))
                    {
                        Log.LogDebug("TfsWorkItemEndPoint::GetTfsCollection: Using PAT Authentication ", _Options.Organisation);
                        vssCredentials = new VssBasicCredential(string.Empty, _Options.AccessToken);
                    }
                    else
                    {
                        Log.LogDebug("TfsWorkItemEndPoint::EnsureDataSource: Using Interactive Authentication ", _Options.Organisation);
                        vssCredentials = new VssCredentials();
                    }
                    Log.LogDebug(Microsoft.TeamFoundation.Framework.Common.LocationServiceConstants.ApplicationLocationServiceIdentifier.ToString());
                    Log.LogDebug("TfsWorkItemEndPoint::GetTfsCollection: Connecting to {CollectionUrl} ", _Options.Organisation);
                    _Collection = new TfsTeamProjectCollection(new Uri(_Options.Organisation), vssCredentials);
                    Log.LogDebug("TfsWorkItemEndPoint::GetTfsCollection: Connected ");
                    Log.LogDebug("TfsWorkItemEndPoint::GetTfsCollection: validating security for {@AuthorizedIdentity} ", _Collection.AuthorizedIdentity);
                    _Collection.EnsureAuthenticated();
                    timer.Stop();
                    Log.LogInformation("TfsWorkItemEndPoint::GetTfsCollection: Access granted to {CollectionUrl} for {Name} ({Account})", _Options.Organisation, _Collection.AuthorizedIdentity.DisplayName, _Collection.AuthorizedIdentity.UniqueName);
                    Telemetry.TrackDependency(new DependencyTelemetry("TfsObjectModel", _Options.Organisation, "GetTfsCollection", null, startTime, timer.Elapsed, "200", true));
                }
                catch (Exception ex)
                {
                    timer.Stop();
                    Telemetry.TrackDependency(new DependencyTelemetry("TfsObjectModel", _Options.Organisation, "GetTfsCollection", null, startTime, timer.Elapsed, "500", false));
                    Log.LogError(ex, "Unable to connect to {Organisation}", _Options.Organisation);
                    throw;
                }
            }
            return _Collection;
        }
    }
}