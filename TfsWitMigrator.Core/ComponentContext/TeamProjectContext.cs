using Microsoft.TeamFoundation.Client;
using System;
using System.Diagnostics;

namespace TfsWitMigrator.Core
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
                Trace.WriteLine(string.Format("validating security for {0} ", _Collection.AuthorizedIdentity.ToString()));
                _Collection.EnsureAuthenticated();
                Trace.WriteLine(string.Format("Connected to {0} ", _Collection.Uri.ToString()));
            }            
        }
    }
}