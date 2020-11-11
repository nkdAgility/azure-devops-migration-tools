using System;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.VisualStudio.Services.Common;
using MigrationTools.EndpointEnrichers;

namespace MigrationTools.Endpoints
{
    public class TfsEndpoint : Endpoint, ITfsEndpointOptions
    {
        private TfsTeamProjectCollection _Collection;
        private Project _Project;
        private WorkItemStore _Store;
        private ITfsEndpointOptions _Options;

        public string AccessToken { get { return _Options.AccessToken; } }
        public string Organisation { get { return _Options.Organisation; } }
        public string Project { get { return _Options.Project; } }

        protected TfsTeamProjectCollection TfsCollection
        {
            get
            {
                return GetTfsCollection();
            }
        }

        protected WorkItemStore TfsStore
        {
            get
            {
                return GetWorkItemStore(TfsCollection, WorkItemStoreFlags.BypassRules);
            }
        }

        protected Project TfsProject
        {
            get { return GetTfsProject(); }
        }

        public Uri TfsProjectUri
        {
            get { return TfsProject.Uri; }
        }

        public override int Count => 0;

        protected TfsEndpoint(EndpointEnricherContainer endpointEnrichers, IServiceProvider services, ITelemetryLogger telemetry, ILogger<Endpoint> logger) : base(endpointEnrichers, services, telemetry, logger)
        {
        }

        public override void Configure(IEndpointOptions options)
        {
            base.Configure(options);
            Log.LogDebug("TfsEndpoint::Configure");
            _Options = (ITfsEndpointOptions)options;
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
                    Telemetry.TrackDependency(new DependencyTelemetry("TfsObjectModel", _Options.Organisation, "GetWorkItemStore", null, startTime, timer.Elapsed, "200", true));
                }
                catch (Exception ex)
                {
                    timer.Stop();
                    Telemetry.TrackDependency(new DependencyTelemetry("TfsObjectModel", _Options.Organisation, "GetWorkItemStore", null, startTime, timer.Elapsed, "500", false));
                    Log.LogError(ex, "Unable to connect to {Organisation} Store", _Options.Organisation);
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
                if (TfsStore.Projects.Contains(_Options.Project))
                {
                    _Project = TfsStore.Projects[_Options.Project];
                }
                else
                {
                    Telemetry.TrackDependency(new DependencyTelemetry("TfsObjectModel", _Options.Organisation, "GetTfsProject", null, startTime, timer.Elapsed, "500", false));
                    Log.LogError(new InvalidFieldValueException(), "Unable to find to {Project}", _Options.Project);
                }
            }
            return _Project;
        }
    }
}