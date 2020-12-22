using Microsoft.Extensions.DependencyInjection;
using MigrationTools.TestExtensions;

namespace MigrationTools.Tests
{
    internal static class ServiceProviderHelper
    {
        internal static ServiceProvider GetServicesV2()
        {
            var services = new ServiceCollection();
            services.AddMigrationToolServicesForUnitTests();
            /////////////////////////////////
            services.AddMigrationToolServices();
            services.AddMigrationToolServicesForClientInMemory();
            services.AddMigrationToolServicesForClientFileSystem();
            services.AddMigrationToolServicesForClientAzureDevOpsObjectModel();
            services.AddMigrationToolServicesForClientAzureDevopsRest();

            return services.BuildServiceProvider();
        }
    }
}