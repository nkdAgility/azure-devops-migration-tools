using MigrationTools.Configuration;
using MigrationTools.DataContracts;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace MigrationTools.Clients
{
    public interface IMigrationClient
    {

        TeamProjectConfig Config { get; }
        IWorkItemMigrationClient WorkItems { get; }
        ITestPlanMigrationClient TestPlans { get; }

        void Configure(TeamProjectConfig config, NetworkCredential credentials = null);

        T GetService<T>();

        [Obsolete]
        object InternalCollection { get; }



    }
}
