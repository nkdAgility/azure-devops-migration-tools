using Microsoft.TeamFoundation.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VstsSyncMigrator.Engine.Configuration;

namespace VstsSyncMigrator.Engine
{
    public interface ITeamProjectContext
    {
        TfsTeamProjectCollection Collection { get; }
        TeamProjectConfig Config { get; }
        void Connect();
    }
}