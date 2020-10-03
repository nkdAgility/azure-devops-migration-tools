using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.TeamFoundation;
using Microsoft.TeamFoundation.Client;
using Microsoft.VisualStudio.Services.Common;
using MigrationTools;
using MigrationTools.Clients;
using MigrationTools.Configuration;
using MigrationTools.DataContracts;
using Serilog;

namespace MigrationTools.Clients.AzureDevops.ObjectModel.Clients
{
    public class MigrationClient : IMigrationClient
    {
        private TeamProjectConfig _config;
        private NetworkCredential _credentials;
        private IWorkItemMigrationClient _workItemClient;
        private ITestPlanMigrationClient _testPlanClient;

        private readonly IServiceProvider _Services;
        private readonly ITelemetryLogger _Telemetry;

        public TeamProjectConfig Config
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
        // if you add Migration Engine in here you will have to fix the infinate loop
        public MigrationClient(ITestPlanMigrationClient testPlanClient,IWorkItemMigrationClient workItemClient, IServiceProvider services, ITelemetryLogger telemetry)
        {
            _testPlanClient = testPlanClient;
            _workItemClient = workItemClient;
            _Services = services;
            _Telemetry = telemetry;
        }


        public void Configure(TeamProjectConfig config, NetworkCredential credentials = null)
        {
            _config = config;
            _credentials = credentials;
            EnsureCollection();
            _workItemClient.Configure(this);
            _testPlanClient.Configure(this);
        }

        private TfsTeamProjectCollection _collection;
        [Obsolete]
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
                          { "Name", Config.Project},
                          { "Target Project", Config.Project},
                          { "Target Collection",Config.Collection.ToString() },
                           { "ReflectedWorkItemID Field Name",Config.ReflectedWorkItemIDFieldName }
                    }, null);
                Stopwatch connectionTimer = Stopwatch.StartNew();
                DateTime start = DateTime.Now;
                Log.Information("MigrationClient: Connecting to {Project} on {Collection}", Config.Project, Config.Collection);
                Log.Verbose("MigrationClient: Connecting to {@Config}", Config);

                if (_credentials == null)
                    _collection = new TfsTeamProjectCollection(Config.Collection);
                else
                    _collection = new TfsTeamProjectCollection(Config.Collection, new VssCredentials(new Microsoft.VisualStudio.Services.Common.WindowsCredential(_credentials)));
                try
                {
                    Log.Debug("MigrationClient: Connected to {CollectionUrl} ", _collection.Uri.ToString());
                    Log.Debug("MigrationClient: validating security for {@AuthorizedIdentity} ", _collection.AuthorizedIdentity);
                    _collection.EnsureAuthenticated();
                    connectionTimer.Stop();
                    _Telemetry.TrackDependency(new DependencyTelemetry("TeamService", "EnsureAuthenticated", start, connectionTimer.Elapsed, true));
                    Log.Information("MigrationClient: Access granted ");
                }
                catch (TeamFoundationServiceUnavailableException ex)
                {
                    _Telemetry.TrackDependency(new DependencyTelemetry("TeamService", "EnsureAuthenticated", start, connectionTimer.Elapsed, false));
                    _Telemetry.TrackException(ex,
                       new Dictionary<string, string> {
                            { "CollectionUrl", Config.Collection.ToString() },
                            { "TeamProjectName",  Config.Project}
                       },
                       new Dictionary<string, double> {
                            { "ConnectionTimer", connectionTimer.ElapsedMilliseconds }
                       });
                    Log.Error(ex, "MigrationClient: Unable to connect to {@Config}", Config);
                    throw;
                }
            }
        }
 
        public T GetService<T>()
        {
            EnsureCollection();
            return _collection.GetService<T>();
        }

    }
}
