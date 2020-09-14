using MigrationTools.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MigrationTools.Core.Engine
{
    public interface ITeamProjectContext
    {
        TeamProjectConfig Config { get; }
        void Connect();



    }
}