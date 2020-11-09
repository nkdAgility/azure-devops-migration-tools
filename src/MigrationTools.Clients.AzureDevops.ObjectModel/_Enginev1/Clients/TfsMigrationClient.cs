using System;
using System.Collections.Generic;
using System.Net;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.TeamFoundation.Client;
using Microsoft.VisualStudio.Services.Common;
using MigrationTools._EngineV1.Configuration;
using Serilog;

namespace MigrationTools._EngineV1.Clients
{
    public class TfsMigrationClient : IMigrationClient
    {
        private TfsTeamProjectConfig _config;
        private TfsTeamProjectCollection _collection;
        private VssCredentials _vssCredentials;
        private NetworkCredential _credentials;
        private IWorkItemMigrationClient _workItemClient;
        private ITestPlanMigrationClient _testPlanClient;

        private readonly IServiceProvider _Services;
        private readonly ITelemetryLogger _Telemetry;

        public TfsTeamProjectConfig TfsConfig
        {
            get
            {
                return _config;
            }
        }

        public IMigrationClientConfig Config
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

        // if you add Migration Engine in here you will have to fix the infinate loop
        public TfsMigrationClient(ITestPlanMigrationClient testPlanClient, IWorkItemMigrationClient workItemClient, IServiceProvider services, ITelemetryLogger telemetry)
        {
            _testPlanClient = testPlanClient;
            _workItemClient = workItemClient;
            _Services = services;
            _Telemetry = telemetry;
        }

        public void Configure(IMigrationClientConfig config, NetworkCredential credentials = null)
        {
            if (config is null)
            {
                throw new ArgumentNullException(nameof(config));
            }
            if (!(config is TfsTeamProjectConfig))
            {
                throw new ArgumentOutOfRangeException(string.Format("{0} needs to be of type {1}", nameof(config), nameof(TfsTeamProjectConfig)));
            }

            _config = (TfsTeamProjectConfig)config;
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
            TfsTeamProjectCollection y;
            try
            {
                if (credentials != null)
                {
                    _vssCredentials = new VssCredentials(new Microsoft.VisualStudio.Services.Common.WindowsCredential(credentials));
                }
                else if (!string.IsNullOrEmpty(TfsConfig.PersonalAccessToken))
                {
                    _vssCredentials = new VssBasicCredential(string.Empty, TfsConfig.PersonalAccessToken);
                }
                else
                {
                    _vssCredentials = new VssCredentials();
                }

                y = new TfsTeamProjectCollection(TfsConfig.Collection, _vssCredentials);

                Log.Debug("MigrationClient: Connecting to {CollectionUrl} ", TfsConfig.Collection);
                Log.Debug("MigrationClient: validating security for {@AuthorizedIdentity} ", y.AuthorizedIdentity);
                y.EnsureAuthenticated();
                timer.Stop();
                Log.Information("MigrationClient: Access granted to {CollectionUrl} for {Name} ({Account})", TfsConfig.Collection, y.AuthorizedIdentity.DisplayName, y.AuthorizedIdentity.UniqueName);
                _Telemetry.TrackDependency(new DependencyTelemetry("TfsObjectModel", TfsConfig.Collection.ToString(), "GetWorkItem", null, startTime, timer.Elapsed, "200", true));
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
                Log.Error(ex, "Unable to configure store");
                throw;
            }
            return y;
        }

        public T GetService<T>()
        {
            EnsureCollection();
            return _collection.GetService<T>();
        }
    }
}