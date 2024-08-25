using System;
using System.Collections.Generic;
using System.Net;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.TeamFoundation;
using Microsoft.TeamFoundation.Client;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using MigrationTools._EngineV1.Configuration;
using MigrationTools.Endpoints;
using Serilog;

namespace MigrationTools._EngineV1.Clients
{
    public class TfsMigrationClient : IMigrationClient
    {
        private TfsTeamProjectEndpointOptions _config;
        private TfsTeamProjectCollection _collection;
        private VssCredentials _vssCredentials;
        private NetworkCredential _credentials;
        private IWorkItemMigrationClient _workItemClient;
        private ITestPlanMigrationClient _testPlanClient;

        private readonly IServiceProvider _Services;
        private readonly ITelemetryLogger _Telemetry;

        public TfsTeamProjectEndpointOptions TfsConfig
        {
            get
            {
                return _config;
            }
        }

        public IEndpointOptions Config
        {
            get
            {
                return _config;
            }
        }

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

        public VssCredentials Credentials => _vssCredentials ??= new VssCredentials();

        public TfsMigrationClient()
        {

        }

        // if you add Migration Engine in here you will have to fix the infinate loop
        public TfsMigrationClient(ITestPlanMigrationClient testPlanClient, IWorkItemMigrationClient workItemClient, IServiceProvider services, ITelemetryLogger telemetry)
        {
            _testPlanClient = testPlanClient;
            _workItemClient = workItemClient;
            _Services = services;
            _Telemetry = telemetry;
        }

        public void Configure(IEndpointOptions config, NetworkCredential credentials = null)
        {
            if (config is null)
            {
                throw new ArgumentNullException(nameof(config));
            }
            if (!(config is TfsTeamProjectEndpointOptions))
            {
                throw new ArgumentOutOfRangeException(string.Format("{0} needs to be of type {1}", nameof(config), nameof(TfsTeamProjectEndpointOptions)));
            }

            _config = (TfsTeamProjectEndpointOptions)config;
            _credentials = credentials;
            EnsureCollection();
            _workItemClient.Configure(this);
            _testPlanClient.Configure(this);
        }

        public object InternalCollection
        {
            get
            {
                return _collection;
            }
        }

        private void EnsureCollection()
        {
            if (_collection == null)
            {
                _Telemetry.TrackEvent("TeamProjectContext.EnsureCollection",
                    new Dictionary<string, string> {
                          { "Name", TfsConfig.Project},
                          { "Target Project", TfsConfig.Project},
                          { "Target Collection",TfsConfig.Collection.ToString() },
                           { "ReflectedWorkItemID Field Name",TfsConfig.ReflectedWorkItemIDFieldName }
                    }, null);
                _collection = GetDependantTfsCollection(_credentials);
            }
        }

        private TfsTeamProjectCollection GetDependantTfsCollection(NetworkCredential credentials)
        {
            var startTime = DateTime.UtcNow;
            var timer = System.Diagnostics.Stopwatch.StartNew();
            TfsTeamProjectCollection y = null;
            try
            {
                Log.Debug("TfsMigrationClient::GetDependantTfsCollection:AuthenticationMode({0})", _config.AuthenticationMode.ToString());
                switch (_config.AuthenticationMode)
                {
                    case AuthenticationMode.AccessToken:
                        Log.Information("Connecting with AccessToken ");
                        var pat = TfsConfig.PersonalAccessToken;
                        if (!string.IsNullOrEmpty(TfsConfig.PersonalAccessTokenVariableName))
                        {
                            pat = Environment.GetEnvironmentVariable(TfsConfig.PersonalAccessTokenVariableName);
                        }
                        _vssCredentials = new VssBasicCredential(string.Empty, pat);
                        y = new TfsTeamProjectCollection(TfsConfig.Collection, _vssCredentials);
                        break;

                    case AuthenticationMode.Windows:
                        Log.Information("Connecting with NetworkCredential passes on CommandLine ");
                        if (credentials is null)
                        {
                            throw new InvalidOperationException("If AuthenticationMode = Windows then you must pass credentails on the command line.");
                        }
                        _vssCredentials = new VssCredentials(new Microsoft.VisualStudio.Services.Common.WindowsCredential(credentials));
                        y = new TfsTeamProjectCollection(TfsConfig.Collection, _vssCredentials);
                        break;

                    case AuthenticationMode.Prompt:
                        Log.Information("Prompting for credentials ");
                        y = new TfsTeamProjectCollection(TfsConfig.Collection);
                        break;

                    default:
                        Log.Information("Setting _vssCredentials to Null ");
                        y = new TfsTeamProjectCollection(TfsConfig.Collection);
                        break;
                }
                Log.Debug("MigrationClient: Connecting to {CollectionUrl} ", TfsConfig.Collection);
                Log.Verbose("MigrationClient: validating security for {@AuthorizedIdentity} ", y.AuthorizedIdentity);
                y.EnsureAuthenticated();
                timer.Stop();
                Log.Information("Access granted to {CollectionUrl} for {Name} ({Account})", TfsConfig.Collection, y.AuthorizedIdentity.DisplayName, y.AuthorizedIdentity.UniqueName);
                _Telemetry.TrackDependency(new DependencyTelemetry("TfsObjectModel", TfsConfig.Collection.ToString(), "GetWorkItem", null, startTime, timer.Elapsed, "200", true));
            }
            catch (TeamFoundationServerUnauthorizedException ex)
            {
                timer.Stop();
                _Telemetry.TrackDependency(new DependencyTelemetry("TfsObjectModel", TfsConfig.Collection.ToString(), "GetWorkItem", null, startTime, timer.Elapsed, "401", false));
                Log.Error(ex, "Unable to configure store: Check persmissions and credentials for {AuthenticationMode}!", _config.AuthenticationMode);
                Environment.Exit(-1);
            }
            catch (Exception ex)
            {
                timer.Stop();
                _Telemetry.TrackDependency(new DependencyTelemetry("TfsObjectModel", TfsConfig.Collection.ToString(), "GetWorkItem", null, startTime, timer.Elapsed, "500", false));
                _Telemetry.TrackException(ex,
                       new Dictionary<string, string> {
                            { "CollectionUrl", TfsConfig.Collection.ToString() },
                            { "TeamProjectName",  TfsConfig.Project}
                       },
                       new Dictionary<string, double> {
                            { "Time",timer.ElapsedMilliseconds }
                       });
                Log.Error("Unable to configure store: Check persmissions and credentials for {AuthenticationMode}: " + ex.Message, _config.AuthenticationMode);
                switch (_config.AuthenticationMode)
                {
                    case AuthenticationMode.AccessToken:
                        Log.Error("The PAT MUST be 'full access' for it to work with the Object Model API.");
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