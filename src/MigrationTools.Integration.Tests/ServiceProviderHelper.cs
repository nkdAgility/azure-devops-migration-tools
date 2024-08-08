using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MigrationTools.Endpoints;
using MigrationTools.Helpers.Tests;
using MigrationTools.Services;
using MigrationTools.TestExtensions;

namespace MigrationTools.Tests
{
    internal static class ServiceProviderHelper
    {
        internal static ServiceProvider GetServicesV2()
        {
            var configurationBuilder = new ConfigurationBuilder();

            var configuration = configurationBuilder.Build();
            var services = new ServiceCollection();
            services.AddMigrationToolServicesForUnitTests();
            /////////////////////////////////
            services.AddMigrationToolServices();
            services.AddMigrationToolServicesForClientInMemory();
            services.AddMigrationToolServicesForClientFileSystem();
            services.AddMigrationToolServicesForClientAzureDevOpsObjectModel(configuration);
            services.AddMigrationToolServicesForClientAzureDevopsRest(configuration);
            AddEndpoint(services, "Source", "migrationSource1");
            AddEndpoint(services, "Target", "migrationTarget1");

            services.AddSingleton<IMigrationToolVersionInfo, FakeMigrationToolVersionInfo>();
            services.AddSingleton<IMigrationToolVersion, FakeMigrationToolVersion>();

            return services.BuildServiceProvider();
        }

        private static void AddEndpoint(IServiceCollection services, string name, string project)
        {
            services.AddMigrationToolsEndpoint(name, (provider) =>
            {
                var options = GetTfsWorkItemEndPointOptions(project);
                var endpoint = provider.GetRequiredService<TfsWorkItemEndpoint>();
                endpoint.Configure(options);
                return endpoint;
            });
        }

        private static TfsWorkItemEndpointOptions GetTfsWorkItemEndPointOptions(string project)
        {
            return new TfsWorkItemEndpointOptions()
            {
                Organisation = "https://dev.azure.com/nkdagility-preview/",
                Project = project,
                AuthenticationMode = AuthenticationMode.AccessToken,
                AccessToken = TestingConstants.AccessToken,
                Query = new Options.QueryOptions()
                {
                    Query = "SELECT [System.Id], [System.Tags] " +
                            "FROM WorkItems " +
                            "WHERE [System.TeamProject] = @TeamProject " +
                                "AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan') " +
                            "ORDER BY [System.ChangedDate] desc",
                    Parameters = new Dictionary<string, string>() { { "TeamProject", "migrationSource1" } }
                }
            };
        }
    }
}