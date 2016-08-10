using Microsoft.TeamFoundation.Client;
using System;
using System.Diagnostics;
using Microsoft.TeamFoundation;
using Microsoft.ApplicationInsights;

namespace VSTS.DataBulkEditor.Engine
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
                Trace.WriteLine("Creating TfsTeamProjectCollection Object ");
                _Collection = new TfsTeamProjectCollection(_CollectionUrl);
                try
                {
                    _Collection.EnsureAuthenticated();
                }
                catch (TeamFoundationServiceUnavailableException ex)
                {
                    TelemetryClient tc = new TelemetryClient();
                    tc.TrackException(ex);
                    Trace.TraceWarning(string.Format("  [EXCEPTION] {0}", ex.Message));
                    throw ex;
                }                
                Trace.WriteLine(string.Format("validating security for {0} ", _Collection.AuthorizedIdentity.ToString()));
                Trace.WriteLine(string.Format("Connected to {0} ", _Collection.Uri.ToString()));
            }            
        }
    }
}