using System.Net;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;
using MigrationTools._EngineV1.Configuration;
using MigrationTools.Endpoints;
using MigrationTools.Endpoints.Infrastructure;

namespace MigrationTools._EngineV1.Clients
{
    // TODO: Rename IMigrationClient to ITfsTeamProjectEndpoint
    public interface IMigrationClient
    {

        IWorkItemMigrationClient WorkItems { get; }
        ITestPlanMigrationClient TestPlans { get; }

        VssCredentials Credentials { get; }

        T GetService<T>();
        T GetClient<T>() where T : IVssHttpClient;

        object InternalCollection { get; }
    }
}