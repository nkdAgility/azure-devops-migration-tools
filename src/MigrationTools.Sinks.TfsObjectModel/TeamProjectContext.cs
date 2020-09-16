using Microsoft.TeamFoundation.Client;
using System;
using System.Diagnostics;
using Microsoft.TeamFoundation;
using Microsoft.VisualStudio.Services.Common;
using System.Collections.Generic;
using System.Net;
using MigrationTools.Core.Configuration;
using MigrationTools.Core.Engine;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.ApplicationInsights;

namespace MigrationTools.Sinks.TfsObjectModel
{
    public class TeamProjectContext : ITeamProjectContext
    {
        private TeamProjectConfig _config;
        private readonly TelemetryClient _Telemetry;
        private TfsTeamProjectCollection _Collection;
        private bool bypassRules = true;
        private WorkItemStore _wistore;
        private Dictionary<int, WorkItem> foundWis;
        private VssCredentials _credentials;

        private WorkItemStore Store { get { return _wistore; } }
        public TeamProjectConfig Config { get { return _config; } }

        public TeamProjectContext(TelemetryClient telemetry)
        {
          _Telemetry = telemetry;
        }

        public void Connect(TeamProjectConfig config)
        {
            _config = config;
            Connect();
        }

        public void Connect(TeamProjectConfig config, NetworkCredential credentials)
        {
            _config = config;
            _credentials = new VssCredentials(new Microsoft.VisualStudio.Services.Common.WindowsCredential(credentials)); ;
            Connect();
        }

        private void Connect()
        {
            if (_Collection == null)
            {
                _Telemetry.TrackEvent("TeamProjectContext.Connect",
                    new Dictionary<string, string> {
                          { "Name", Config.Project},
                          { "Target Project", Config.Project},
                          { "Target Collection",Config.Collection.ToString() },
                           { "ReflectedWorkItemID Field Name",Config.ReflectedWorkItemIDFieldName }
                    });
                Stopwatch connectionTimer = Stopwatch.StartNew();
				DateTime start = DateTime.Now;
                Trace.WriteLine("Creating TfsTeamProjectCollection Object ");

                if (_credentials == null)
                    _Collection = new TfsTeamProjectCollection(Config.Collection);
                else
                    _Collection = new TfsTeamProjectCollection(Config.Collection, _credentials);
                
                try
                {
                    Trace.WriteLine(string.Format("Connected to {0} ", _Collection.Uri.ToString()));
                    Trace.WriteLine(string.Format("validating security for {0} ", _Collection.AuthorizedIdentity.ToString()));
                    _Collection.EnsureAuthenticated();
                    connectionTimer.Stop();
                    _Telemetry.TrackDependency("TeamService", "EnsureAuthenticated", start, connectionTimer.Elapsed, true);
                    Trace.TraceInformation(string.Format(" Access granted "));
                }
                catch (TeamFoundationServiceUnavailableException ex)
                {
                    _Telemetry.TrackDependency("TeamService", "EnsureAuthenticated", start, connectionTimer.Elapsed, false);
                    _Telemetry.TrackException(ex,
                       new Dictionary<string, string> {
                            { "CollectionUrl", Config.Collection.ToString() },
                            { "TeamProjectName",  Config.Project}
                       },
                       new Dictionary<string, double> {
                            { "ConnectionTimer", connectionTimer.ElapsedMilliseconds }
                       });
                    Trace.TraceWarning($"  [EXCEPTION] {ex}");
                    throw;
                }
            }
            ConnectStore();
        }

        private void ConnectStore()
        {
            var startTime = DateTime.UtcNow;
            var timer = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                _wistore = new WorkItemStore(_Collection, bypassRules ? WorkItemStoreFlags.BypassRules : WorkItemStoreFlags.None);
                timer.Stop();
                _Telemetry.TrackDependency("TeamService", "GetWorkItemStore", startTime, timer.Elapsed, true);
            }
            catch (Exception ex)
            {
                timer.Stop();
                _Telemetry.TrackDependency("TeamService", "GetWorkItemStore", startTime, timer.Elapsed, false);
                _Telemetry.TrackException(ex,
                       new Dictionary<string, string> {
                            { "CollectionUrl", _Collection.Uri.ToString() }
                       },
                       new Dictionary<string, double> {
                            { "Time",timer.ElapsedMilliseconds }
                       });
                Trace.TraceWarning($"  [EXCEPTION] {ex}");
                throw;
            }
            foundWis = new Dictionary<int, WorkItem>();
        }



    }
}