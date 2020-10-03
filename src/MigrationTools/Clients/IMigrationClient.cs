using System;
using System.Net;
using MigrationTools.Configuration;

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