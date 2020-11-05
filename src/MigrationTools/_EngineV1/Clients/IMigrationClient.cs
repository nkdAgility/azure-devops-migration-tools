using System;
using System.Net;
using MigrationTools.Configuration;

namespace MigrationTools._EngineV1.Clients
{
    public interface IMigrationClient
    {
        IMigrationClientConfig Config { get; }
        IWorkItemMigrationClient WorkItems { get; }
        ITestPlanMigrationClient TestPlans { get; }

        void Configure(IMigrationClientConfig config, NetworkCredential credentials = null);

        T GetService<T>();

        [Obsolete]
        object InternalCollection { get; }
    }
}