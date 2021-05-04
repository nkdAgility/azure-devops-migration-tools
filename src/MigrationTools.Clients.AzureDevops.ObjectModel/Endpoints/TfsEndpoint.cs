using System;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.VisualStudio.Services.Common;
using MigrationTools.EndpointEnrichers;

namespace MigrationTools.Endpoints
{
    public class TfsEndpoint : GenericTfsEndpoint<TfsEndpointOptions>
    {
        public TfsEndpoint(EndpointEnricherContainer endpointEnrichers, ITelemetryLogger telemetry, ILogger<TfsEndpoint> logger)
            : base(endpointEnrichers, telemetry, logger)
        {
        }
    }

    public class GenericTfsEndpoint<TTfsOptions> : Endpoint<TTfsOptions>
        where TTfsOptions : TfsEndpointOptions
    {
        private TfsTeamProjectCollection _Collection;
        private Project _Project;
        private WorkItemStore _Store;

        public string Project => Options.Project;

        internal TfsTeamProjectCollection TfsCollection
        {
            get
            {
                return GetTfsCollection();
            }
        }

        internal WorkItemStore TfsStore
        {
            get
            {
                return GetWorkItemStore(TfsCollection, WorkItemStoreFlags.BypassRules);
            }
        }

        internal Project TfsProject
        {
            get { return GetTfsProject(); }
        }

        public Uri TfsProjectUri
        {
            get { return TfsProject.Uri; }
        }

        public override int Count => 0;

        public GenericTfsEndpoint(EndpointEnricherContainer endpointEnrichers, ITelemetryLogger telemetry, ILogger<GenericTfsEndpoint<TTfsOptions>> logger)
            : base(endpointEnrichers, telemetry, logger)
        {
        }

        public override void Configure(TTfsOptions options)
        {
            base.Configure(options);
            Log.LogDebug("TfsEndpoint::Configure");
            if (string.IsNullOrEmpty(Options.Organisation))
            {
                throw new ArgumentNullException(nameof(Options.Organisation));
            }
            if (string.IsNullOrEmpty(Options.Project))
            {
                throw new ArgumentNullException(nameof(Options.Project));
            }
            if (string.IsNullOrEmpty(Options.AccessToken) && Options.AuthenticationMode == AuthenticationMode.AccessToken)
            {
                throw new ArgumentNullException(nameof(Options.AccessToken));
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
                    Log.LogDebug("TfsWorkItemEndPoint::GetTfsCollection:AuthenticationMode({0})", Options.AuthenticationMode.ToString());
                    switch (Options.AuthenticationMode)
                    {
                        case AuthenticationMode.AccessToken:
                            Log.LogDebug("TfsWorkItemEndPoint::GetTfsCollection: Connecting Using PAT Authentication ", Options.Organisation);
                            vssCredentials = new VssBasicCredential(string.Empty, Options.AccessToken);
                            _Collection = new TfsTeamProjectCollection(new Uri(Options.Organisation), vssCredentials);
                            break;

                        case AuthenticationMode.Prompt:
                            Log.LogDebug("TfsWorkItemEndPoint::EnsureDataSource: Connecting Using Interactive Authentication ", Options.Organisation);
                            _Collection = new TfsTeamProjectCollection(new Uri(Options.Organisation));
                            break;

                        default:
                            Log.LogDebug("TfsWorkItemEndPoint::EnsureDataSource: Connecting Using Interactive Authentication ", Options.Organisation);
                            _Collection = new TfsTeamProjectCollection(new Uri(Options.Organisation));
                            break;
                    }
                    Log.LogDebug("TfsWorkItemEndPoint::GetTfsCollection: Connected ");
                    Log.LogDebug("TfsWorkItemEndPoint::GetTfsCollection: validating security for {@AuthorizedIdentity} ", _Collection.AuthorizedIdentity);
                    _Collection.EnsureAuthenticated();
                    timer.Stop();
                    Log.LogInformation("TfsWorkItemEndPoint::GetTfsCollection: Access granted to {CollectionUrl} for {Name} ({Account})", Options.Organisation, _Collection.AuthorizedIdentity.DisplayName, _Collection.AuthorizedIdentity.UniqueName);
                    Telemetry.TrackDependency(new DependencyTelemetry("TfsObjectModel", Options.Organisation, "GetTfsCollection", null, startTime, timer.Elapsed, "200", true));
                }
                catch (Exception ex)
                {
                    timer.Stop();
                    Telemetry.TrackDependency(new DependencyTelemetry("TfsObjectModel", Options.Organisation, "GetTfsCollection", null, startTime, timer.Elapsed, "500", false));
                    Log.LogError(ex, "Unable to connect to {Organisation}", Options.Organisation);
                    throw;
                }
            }
            return _Collection;
        }

        private WorkItemStore GetWorkItemStore(TfsTeamProjectCollection tfs, WorkItemStoreFlags bypassRules)
        {
            if (_Store is null)
            {
                var startTime = DateTime.UtcNow;
                var timer = System.Diagnostics.Stopwatch.StartNew();
                try
                {
                    _Store = new WorkItemStore(tfs, bypassRules);
                    timer.Stop();
                    Telemetry.TrackDependency(new DependencyTelemetry("TfsObjectModel", Options.Organisation, "GetWorkItemStore", null, startTime, timer.Elapsed, "200", true));
                }
                catch (Exception ex)
                {
                    timer.Stop();
                    Telemetry.TrackDependency(new DependencyTelemetry("TfsObjectModel", Options.Organisation, "GetWorkItemStore", null, startTime, timer.Elapsed, "500", false));
                    Log.LogError(ex, "Unable to connect to {Organisation} Store", Options.Organisation);
                    throw;
                }
            }

            return _Store;
        }

        private Project GetTfsProject()
        {
            var startTime = DateTime.UtcNow;
            var timer = System.Diagnostics.Stopwatch.StartNew();
            if (_Project is null)
            {
                if (TfsStore.Projects.Contains(Options.Project))
                {
                    _Project = TfsStore.Projects[Options.Project];
                }
                else
                {
                    Telemetry.TrackDependency(new DependencyTelemetry("TfsObjectModel", Options.Organisation, "GetTfsProject", null, startTime, timer.Elapsed, "500", false));
                    Log.LogError(new InvalidFieldValueException(), "Unable to find to {Project}", Options.Project);
                }
            }
            return _Project;
        }
    }
}