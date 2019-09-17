using Microsoft.TeamFoundation.Client;
using System;
using System.Diagnostics;
using Microsoft.TeamFoundation;
using Microsoft.ApplicationInsights;
using System.Collections.Generic;
using VstsSyncMigrator.Engine.Configuration;

namespace VstsSyncMigrator.Engine
{
    public class TeamProjectContext : ITeamProjectContext
    {
        private TeamProjectConfig _config;
        private TfsTeamProjectCollection _Collection;

        public TfsTeamProjectCollection Collection
        {
            get
            {
                Connect();
                return _Collection;
            }
        }

        public TeamProjectConfig Config
        {
            get
            {
                return _config;
            }
        }

        public TeamProjectContext(TeamProjectConfig config)
        {

            this._config = config;
        }

        public void Connect()
        {
            if (_Collection == null)
            {
                Telemetry.Current.TrackEvent("TeamProjectContext.Connect",
                    new Dictionary<string, string> {
                          { "Name", Config.Name},
                          { "Target Project", Config.Name},
                          { "Target Collection",Config.Collection.ToString() },
                           { "ReflectedWorkItemID Field Name",Config.ReflectedWorkItemIDFieldName }
                    });
                Stopwatch connectionTimer = Stopwatch.StartNew();
				DateTime start = DateTime.Now;
                Trace.WriteLine("Creating TfsTeamProjectCollection Object ");
                    _Collection = new TfsTeamProjectCollection(Config.Collection);
                try
                {
                    Trace.WriteLine(string.Format("Connected to {0} ", _Collection.Uri.ToString()));
                    Trace.WriteLine(string.Format("validating security for {0} ", _Collection.AuthorizedIdentity.ToString()));
                    _Collection.EnsureAuthenticated();
                    connectionTimer.Stop();
                    Telemetry.Current.TrackDependency("TeamService", "EnsureAuthenticated", start, connectionTimer.Elapsed, true);
                    Trace.TraceInformation(string.Format(" Access granted "));
                }
                catch (TeamFoundationServiceUnavailableException ex)
                {
                    Telemetry.Current.TrackDependency("TeamService", "EnsureAuthenticated", start, connectionTimer.Elapsed, false);
                    Telemetry.Current.TrackException(ex,
                       new Dictionary<string, string> {
                            { "CollectionUrl", Config.Collection.ToString() },
                            { "TeamProjectName",  Config.Name}
                       },
                       new Dictionary<string, double> {
                            { "ConnectionTimer", connectionTimer.ElapsedMilliseconds }
                       });
                    Trace.TraceWarning($"  [EXCEPTION] {ex}");
                    throw;
                }
            }            
        }
    }
}