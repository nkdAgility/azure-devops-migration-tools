using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.VisualStudio.Services.Common;
using MigrationTools.EndpointEnrichers;
using MigrationTools.Services;

namespace MigrationTools.Endpoints
{
    public class TfsEndpoint : GenericTfsEndpoint<TfsEndpointOptions>
    {
        public TfsEndpoint(IOptions<TfsEndpointOptions> options, EndpointEnricherContainer endpointEnrichers, IServiceProvider serviceProvider, ITelemetryLogger telemetry, ILogger<Endpoint<TfsEndpointOptions>> logger) : base(options, endpointEnrichers, serviceProvider, telemetry, logger)
        {
        }
    }

    public class GenericTfsEndpoint<TTfsOptions> : Endpoint<TTfsOptions>
        where TTfsOptions : TfsEndpointOptions
    {
        private TfsTeamProjectCollection _Collection;
        private Project _Project;
        private WorkItemStore _Store;

        public GenericTfsEndpoint(IOptions<TTfsOptions> options, EndpointEnricherContainer endpointEnrichers, IServiceProvider serviceProvider, ITelemetryLogger telemetry, ILogger<Endpoint<TTfsOptions>> logger) : base(options, endpointEnrichers, serviceProvider, telemetry, logger)
        {
        }

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




        private TfsTeamProjectCollection GetTfsCollection()
        {
            using (var activity = ActivitySourceProvider.ActivitySource.StartActivity("GetTfsCollection", ActivityKind.Client))
            {
                activity?.SetTagsFromOptions(Options);
                activity?.SetTag("url.full", Options.Collection);
                activity?.SetTag("server.address", Options.Collection);
                activity?.SetTag("http.request.method", "GET");
                activity?.SetTag("migrationtools.client", "TfsObjectModel");
                activity?.SetEndTime(activity.StartTimeUtc.AddSeconds(10));

                if (_Collection is null)
                {
                    VssCredentials vssCredentials;
                    try
                    {
                        Log.LogDebug("TfsWorkItemEndPoint::GetTfsCollection:AuthenticationMode({0})", Options?.Authentication?.AuthenticationMode.ToString());
                        switch (Options?.Authentication?.AuthenticationMode)
                        {
                            case AuthenticationMode.AccessToken:
                                Log.LogDebug("TfsWorkItemEndPoint::GetTfsCollection: Connecting Using PAT Authentication ", Options.Collection);
                                vssCredentials = new VssBasicCredential(string.Empty, Options?.Authentication?.AccessToken);
                                _Collection = new TfsTeamProjectCollection(Options.Collection, vssCredentials);
                                break;

                            case AuthenticationMode.Prompt:
                                Log.LogDebug("TfsWorkItemEndPoint::EnsureDataSource: Connecting Using Interactive Authentication ", Options.Collection);
                                _Collection = new TfsTeamProjectCollection(Options.Collection);
                                break;

                            default:
                                Log.LogDebug("TfsWorkItemEndPoint::EnsureDataSource: Connecting Using Interactive Authentication ", Options.Collection);
                                _Collection = new TfsTeamProjectCollection(Options.Collection);
                                break;
                        }
                        Log.LogDebug("TfsWorkItemEndPoint::GetTfsCollection: Connected ");
                        Log.LogDebug("TfsWorkItemEndPoint::GetTfsCollection: validating security for {@AuthorizedIdentity} ", _Collection.AuthorizedIdentity);
                        _Collection.EnsureAuthenticated();

                        Log.LogInformation("TfsWorkItemEndPoint::GetTfsCollection: Access granted to {CollectionUrl} for {Name} ({Account})", Options.Collection, _Collection.AuthorizedIdentity.DisplayName, _Collection.AuthorizedIdentity.UniqueName);
                        activity?.Stop();
                        activity?.SetStatus(ActivityStatusCode.Ok);
                        activity?.SetTag("http.response.status_code", "200");
                    }
                    catch (Exception ex)
                    {

                        activity?.Stop();
                        activity?.SetStatus(ActivityStatusCode.Error);
                        activity?.SetTag("http.response.status_code", "500");
                        Telemetry.TrackException(ex, null);
                        Log.LogError(ex, "Unable to connect to {Organisation}", Options?.Collection);
                        throw;
                    }
                }
                return _Collection;
            }
        }

        private WorkItemStore GetWorkItemStore(TfsTeamProjectCollection tfs, WorkItemStoreFlags bypassRules)
        {
            if (_Store is null)
            {
                using (var activity = ActivitySourceProvider.ActivitySource.StartActivity("GetWorkItemStore", ActivityKind.Client))
                {
                    activity?.SetTagsFromOptions(Options);
                    activity?.SetTag("url.full", Options?.Collection);
                    activity?.SetTag("server.address", Options?.Collection);
                    activity?.SetTag("http.request.method", "GET");
                    activity?.SetTag("migrationtools.client", "TfsObjectModel");
                    activity?.SetEndTime(activity.StartTimeUtc.AddSeconds(10));
                    try
                    {
                        _Store = new WorkItemStore(tfs, bypassRules);
                        activity?.Stop();
                        activity?.SetStatus(ActivityStatusCode.Ok);
                        activity?.SetTag("http.response.status_code", "200");
                    }
                    catch (Exception ex)
                    {
                        Telemetry.TrackException(ex, null);
                        Log.LogError(ex, "Unable to connect to {Organisation} Store", Options.Collection);
                        throw;
                    }
                    finally
                    {
                        activity?.Stop();
                        activity?.SetStatus(ActivityStatusCode.Error);
                        activity?.SetTag("http.response.status_code", "500");
                    }
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
                using (var activity = ActivitySourceProvider.ActivitySource.StartActivity("GetTfsProject", ActivityKind.Client))
                {
                    activity?.SetTagsFromOptions(Options);
                    activity?.SetTag("url.full", Options.Collection);
                    activity?.SetTag("server.address", Options.Collection);
                    activity?.SetTag("http.request.method", "GET");
                    activity?.SetTag("migrationtools.client", "TfsObjectModel");
                    activity?.SetEndTime(activity.StartTimeUtc.AddSeconds(10));


                    if (TfsStore.Projects.Contains(Options.Project))
                    {
                        _Project = TfsStore.Projects[Options.Project];
                        activity?.Stop();
                        activity?.SetStatus(ActivityStatusCode.Ok);
                        activity?.SetTag("http.response.status_code", "200");
                    }
                    else
                    {
                        activity?.Stop();
                        activity?.SetStatus(ActivityStatusCode.Error);
                        Log.LogError(new InvalidFieldValueException(), "Unable to find to {Project}", Options.Project);
                    }
                }
            }
            return _Project;
        }
    }
}