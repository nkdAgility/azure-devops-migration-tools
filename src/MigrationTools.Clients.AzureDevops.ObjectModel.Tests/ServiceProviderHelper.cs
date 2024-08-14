using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MigrationTools.Endpoints;
using MigrationTools.Helpers.Tests;
using MigrationTools.Services;
using MigrationTools.TestExtensions;

namespace MigrationTools.Tests
{
    public static class ServiceProviderHelper
    {
        public static ServiceProvider GetServices()
        {
            var configuration = new ConfigurationBuilder().Build();
            var services = new ServiceCollection();
            services.AddMigrationToolServicesForUnitTests();

            services.AddMigrationToolServices(configuration);
            services.AddMigrationToolServicesForClientAzureDevOpsObjectModel(configuration);
            services.AddMigrationToolServicesForClientLegacyAzureDevOpsObjectModel();
            services.AddMigrationToolServicesLegacy();
            AddTfsEndpoint(services, "Source", "migrationSource1");
            AddTfsEndpoint(services, "Target", "migrationTarget1");

            AddTfsTeamEndpoint(services, "TfsTeamSettingsSource", "migrationSource1");
            AddTfsTeamEndpoint(services, "TfsTeamSettingsTarget", "migrationTarget1");

            services.AddSingleton<IMigrationToolVersionInfo, FakeMigrationToolVersionInfo>();
            services.AddSingleton<IMigrationToolVersion, FakeMigrationToolVersion>();

            return services.BuildServiceProvider();
        }

        private static void AddTfsTeamEndpoint(IServiceCollection services, string name, string project)
        {
            services.AddMigrationToolsEndpoint(name, (provider) =>
            {
                var options = GetTfsTeamEndPointOptions(project);
                var endpoint = provider.GetRequiredService<TfsTeamSettingsEndpoint>();
                endpoint.Configure(options);
                return endpoint;
            });
        }

        private static TfsTeamSettingsEndpointOptions GetTfsTeamEndPointOptions(string project)
        {
            return new TfsTeamSettingsEndpointOptions()
            {
                Organisation = "https://dev.azure.com/nkdagility-preview/",
                Project = project,
                AuthenticationMode = AuthenticationMode.AccessToken,
                AccessToken = TestingConstants.AccessToken,
            };
        }

        private static void AddTfsEndpoint(IServiceCollection services, string name, string project)
        {
            services.AddMigrationToolsEndpoint(name, (provider) =>
            {
                var options = GetTfsEndPointOptions(project);
                var endpoint = provider.GetRequiredService<TfsEndpoint>();
                endpoint.Configure(options);
                return endpoint;
            });
        }

        private static TfsEndpointOptions GetTfsEndPointOptions(string project)
        {
            return new TfsEndpointOptions()
            {
                Organisation = "https://dev.azure.com/nkdagility-preview/",
                Project = project,
                AuthenticationMode = AuthenticationMode.AccessToken,
                AccessToken = TestingConstants.AccessToken,
            };
        }
    }
}