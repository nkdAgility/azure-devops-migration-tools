using Microsoft.TeamFoundation.Client;
using System;
using System.Diagnostics;
using Microsoft.TeamFoundation;
using Microsoft.ApplicationInsights;
using System.Collections.Generic;

namespace VstsSyncMigrator.Engine
{
    public class TeamProjectContext : ITeamProjectContext
    {
        private Uri _CollectionUrl;
        private TfsTeamProjectCollection _Collection;
        private string _TeamProjectName;

        public TfsTeamProjectCollection Collection
        {
            get
            {
                Connect();
                return _Collection;
            }
        }

        public string Name
        {
            get
            {
                return _TeamProjectName;
            }
        }

        public TeamProjectContext(Uri collectionUrl  , string teamProjectName)
        {

            this._CollectionUrl = collectionUrl;
            this._TeamProjectName = teamProjectName;
        }

        public void Connect()
        {
            if (_Collection == null)
            {
                Telemetry.Current.TrackEvent("TeamProjectContext.Connect",
                    new Dictionary<string, string> {
                          { "Name", Name},
                          { "Target Project", this._TeamProjectName},
                          { "Target Collection", this._CollectionUrl.ToString() }
                    });
                Stopwatch connectionTimer = Stopwatch.StartNew();
				DateTime start = DateTime.Now;
                Trace.WriteLine("Creating TfsTeamProjectCollection Object ");
                    _Collection = new TfsTeamProjectCollection(_CollectionUrl);
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
                            { "CollectionUrl", _CollectionUrl.ToString() },
                            { "TeamProjectName",  _TeamProjectName}
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