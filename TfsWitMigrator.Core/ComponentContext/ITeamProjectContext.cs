using Microsoft.TeamFoundation.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSTS.DataBulkEditor.Engine
{
    public interface ITeamProjectContext
    {
        TfsTeamProjectCollection Collection { get; }
        string Name { get; }
        void Connect();
    }
}