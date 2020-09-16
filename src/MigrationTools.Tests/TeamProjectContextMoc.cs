using MigrationTools.Core.Configuration;
using MigrationTools.Core.Engine;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace MigrationTools.Tests
{
    public class TeamProjectContextMoc : ITeamProjectContext
    {
        TeamProjectConfig ITeamProjectContext.Config 
        { get { return null; }
        }

        public void Connect(TeamProjectConfig config)
        {
            
        }

        public void Connect(TeamProjectConfig config, NetworkCredential credentials)
        {
            
        }
    }
}
