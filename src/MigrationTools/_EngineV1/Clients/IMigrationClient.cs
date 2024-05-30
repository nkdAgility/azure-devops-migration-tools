using System.Net;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using MigrationTools._EngineV1.Configuration;

namespace MigrationTools._EngineV1.Clients
{
    public interface IMigrationClient
    {
        IMigrationClientConfig Config { get; }
        IWorkItemMigrationClient WorkItems { get; }
        ITestPlanMigrationClient TestPlans { get; }

        VssCredentials Credentials { get; }

        void Configure(IMigrationClientConfig config, NetworkCredential credentials = null);

        T GetService<T>();
        T GetClient<T>() where T : IVssHttpClient;

        object InternalCollection { get; }
    }
}