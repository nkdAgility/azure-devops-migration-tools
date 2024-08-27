using System;
using System.Collections.Generic;
using System.Net;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.TeamFoundation;
using Microsoft.TeamFoundation.Client;
using Microsoft.VisualStudio.Services.Client;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using MigrationTools._EngineV1.Configuration;
using MigrationTools.EndpointEnrichers;
using MigrationTools.Endpoints;
using MigrationTools.Endpoints.Infrastructure;
using MigrationTools.Options;
using Serilog;

namespace MigrationTools._EngineV1.Clients
{
    public class TfsTeamProjectEndpoint : Endpoint<TfsTeamProjectEndpointOptions>, IMigrationClient // TODO: Rename IMigrationClient to ITfsTeamProjectEndpoint
    {
        private TfsTeamProjectCollection _collection;
        private VssCredentials _vssCredentials;
        private IWorkItemMigrationClient _workItemClient;
        private ITestPlanMigrationClient _testPlanClient;

        public TfsTeamProjectEndpoint(
            IOptions<TfsTeamProjectEndpointOptions> options,
            EndpointEnricherContainer endpointEnrichers,
            ITelemetryLogger telemetry,
            ILogger<TfsTeamProjectEndpoint> logger,
            IServiceProvider Services
            ) : base(options, endpointEnrichers, Services, telemetry, logger)
        {
            _testPlanClient = new TfsTestPlanMigrationClient(options);

            _workItemClient = ActivatorUtilities.CreateInstance<TfsWorkItemMigrationClient>(Services, this, options);
            //networkCredentials IOptions<NetworkCredentialsOptions> networkCredentials,
         
        }

        public override int Count => 0;


        public IWorkItemMigrationClient WorkItems
        {
            get
            {
                return _workItemClient;
            }
        }

        public ITestPlanMigrationClient TestPlans
        {
            get
            {
                return _testPlanClient;
            }
        }

        public object InternalCollection
        {
            get
            {
                EnsureCollection();
                return _collection;
            }
        }

        private void EnsureCollection()
        {
            if (_collection == null)
            {
                Telemetry.TrackEvent("TeamProjectContext.EnsureCollection",
                    new Dictionary<string, string> {
                          { "Name", Options.Project},
                          { "Target Project", Options.Project},
                          { "Target Collection",Options.Collection.ToString() },
                           { "ReflectedWorkItemID Field Name",Options.ReflectedWorkItemIDFieldName }
                    }, null);
                _collection = GetDependantTfsCollection();
            }
        }

        private TfsTeamProjectCollection GetDependantTfsCollection()
        {
            var startTime = DateTime.UtcNow;
            var timer = System.Diagnostics.Stopwatch.StartNew();
            TfsTeamProjectCollection y = null;
            try
            {
                Log.LogDebug("TfsMigrationClient::GetDependantTfsCollection:AuthenticationMode({0})", Options.Authentication.AuthenticationMode.ToString());
                switch (Options.Authentication.AuthenticationMode)
                {
                    case AuthenticationMode.AccessToken:
                        Log.LogInformation("Connecting with AccessToken ");
                        if (string.IsNullOrEmpty(Options.Authentication.AccessToken))
                        {
                            Log.LogCritical("You must provide a PAT to use 'AccessToken' as the authentication mode");
                            Environment.Exit(-1);
                        }
                        var pat = Options.Authentication.AccessToken;
                        _vssCredentials = new VssBasicCredential(string.Empty, pat);
                        y = new TfsTeamProjectCollection(Options.Collection, _vssCredentials);
                        break;
                    case AuthenticationMode.Windows:
                        Log.LogInformation("Connecting with NetworkCredential ");
                        if (Options.Authentication.NetworkCredentials == null)
                        {
                            Log.LogCritical("You must set NetworkCredential to use 'Windows' as the authentication mode");
                            Environment.Exit(-1);
                        }
                        var cred = new NetworkCredential(Options.Authentication.NetworkCredentials.UserName, Options.Authentication.NetworkCredentials.Password, Options.Authentication.NetworkCredentials.Domain);
                        _vssCredentials = new VssCredentials(new Microsoft.VisualStudio.Services.Common.WindowsCredential(cred));
                        y = new TfsTeamProjectCollection(Options.Collection, _vssCredentials);
                        break;
                    case AuthenticationMode.Prompt:
                        Log.LogInformation("Prompting for credentials ");
                        _vssCredentials = new VssClientCredentials();
                        _vssCredentials.PromptType = CredentialPromptType.PromptIfNeeded;
                        y = new TfsTeamProjectCollection(Options.Collection, _vssCredentials);
                        break;

                    default:
                        Log.LogInformation("Setting _vssCredentials to Null ");
                        y = new TfsTeamProjectCollection(Options.Collection);
                        break;
                }
                Log.LogDebug("MigrationClient: Connecting to {CollectionUrl} ", Options.Collection);
                Log.LogTrace("MigrationClient: validating security for {@AuthorizedIdentity} ", y.AuthorizedIdentity);
                y.EnsureAuthenticated();
                timer.Stop();
                Log.LogInformation("Access granted to {CollectionUrl} for {Name} ({Account})", Options.Collection, y.AuthorizedIdentity.DisplayName, y.AuthorizedIdentity.UniqueName);
                Telemetry.TrackDependency(new DependencyTelemetry("TfsObjectModel", Options.Collection.ToString(), "GetWorkItem", null, startTime, timer.Elapsed, "200", true));
            }
            catch (TeamFoundationServerUnauthorizedException ex)
            {
                timer.Stop();
                Telemetry.TrackDependency(new DependencyTelemetry("TfsObjectModel", Options.Collection.ToString(), "GetWorkItem", null, startTime, timer.Elapsed, "401", false));
                Log.LogError(ex, "Unable to configure store: Check persmissions and credentials for {AuthenticationMode}!", Options.Authentication.AuthenticationMode);
                Environment.Exit(-1);
            }
            catch (Exception ex)
            {
                timer.Stop();
                Telemetry.TrackDependency(new DependencyTelemetry("TfsObjectModel", Options.Collection.ToString(), "GetWorkItem", null, startTime, timer.Elapsed, "500", false));
                Telemetry.TrackException(ex,
                       new Dictionary<string, string> {
                            { "CollectionUrl", Options.Collection.ToString() },
                            { "TeamProjectName",  Options.Project}
                       },
                       new Dictionary<string, double> {
                            { "Time",timer.ElapsedMilliseconds }
                       });
                Log.LogError("Unable to configure store: Check persmissions and credentials for {AuthenticationMode}: " + ex.Message, Options.Authentication.AuthenticationMode);
                switch (Options.Authentication.AuthenticationMode)
                {
                    case AuthenticationMode.AccessToken:
                        Log.LogError("The PAT MUST be 'full access' for it to work with the Object Model API.");
                        break;
                    default:
                        break;
                }
                Environment.Exit(-1);
            }
            return y;
        }

        public T GetService<T>()
        {
            EnsureCollection();
            return _collection.GetService<T>();
        }

        public T GetClient<T>() where T : IVssHttpClient
        {
            EnsureCollection();
            return _collection.GetClient<T>();
        }
    }
}