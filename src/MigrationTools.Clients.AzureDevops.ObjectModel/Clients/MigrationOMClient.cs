using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using MigrationTools.Core.Clients;
using MigrationTools.Core.Configuration;

namespace MigrationTools.Clients.AzureDevops.ObjectModel.Clients
{
    public class MigrationOMClient : IMigrationClient
    {
        private TeamProjectConfig _config;
        private NetworkCredential _credentials;

        public TeamProjectConfig Config
        {
            get
            {
                return _config;
            }
        }

        public void Configure(TeamProjectConfig config, NetworkCredential credentials = null)
        {
            _config = config;
            _credentials = credentials;
        }




    }
}
