using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MigrationTools.Endpoints;
using MigrationTools.TestExtensions;

namespace MigrationTools.Tests
{
    internal static class ServiceProviderHelper
    {
        internal static ServiceProvider GetServices()
        {
            var configuration = new ConfigurationBuilder().Build();
            var services = new ServiceCollection();
            services.AddMigrationToolServicesForUnitTests();

            services.AddMigrationToolServices();
            services.AddMigrationToolServicesForClientAzureDevopsRest(configuration);
            AddEndpoint(services, "Source", "migrationSource1");
            AddEndpoint(services, "Target", "migrationTarget1");

            return services.BuildServiceProvider();
        }

        private static void AddEndpoint(IServiceCollection services, string name, string project)
        {
            services.AddEndpoint(name, (provider) =>
            {
                var options = GetAzureDevOpsEndpointOptions(project);
                var endpoint = provider.GetRequiredService<AzureDevOpsEndpoint>();
                endpoint.Configure(options);
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