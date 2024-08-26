using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MigrationTools.Endpoints;
using MigrationTools.Services;
using MigrationTools.Services.Shadows;
using MigrationTools.Shadows;

namespace MigrationTools.Tests
{
    internal static class ServiceProviderHelper
    {
        internal static ServiceProvider GetServices()
        {
            var configuration = new ConfigurationBuilder().Build();
            var services = new ServiceCollection();
            services.AddMigrationToolServicesForUnitTests();

            services.AddMigrationToolServices(configuration);
            services.AddMigrationToolServicesForClientAzureDevopsRest(configuration);

            AddEndpoint(services, "Source", "migrationSource1");
            AddEndpoint(services, "Target", "migrationTarget1");

            services.AddSingleton<IMigrationToolVersionInfo, FakeMigrationToolVersionInfo>();
            services.AddSingleton<IMigrationToolVersion, FakeMigrationToolVersion>();

            return services.BuildServiceProvider();
        }

        private static void AddEndpoint(IServiceCollection services, string name, string project)
        {
            services.AddKeyedSingleton(typeof(IEndpoint), name, (sp, key) =>
            {
                var options = GetAzureDevOpsEndpointOptions(project);
                var endpoint = ActivatorUtilities.CreateInstance(sp, typeof(AzureDevOpsEndpoint), options);
                return endpoint;
            });
        }

        private static AzureDevOpsEndpointOptions GetAzureDevOpsEndpointOptions(string project)
        {
            return new AzureDevOpsEndpointOptions()
            {
                Organisation = "https://dev.azure.com/nkdagility-preview/",
                Project = project,
                AuthenticationMode = AuthenticationMode.AccessToken,
                AccessToken = TestingConstants.AccessToken,
            };
        }
    }
}