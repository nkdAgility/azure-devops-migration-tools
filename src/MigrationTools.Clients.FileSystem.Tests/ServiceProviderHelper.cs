using Microsoft.Extensions.DependencyInjection;
using MigrationTools.TestExtensions;

namespace MigrationTools.Tests
{
    internal static class ServiceProviderHelper
    {
        internal static ServiceProvider GetServices()
        {
            var services = new ServiceCollection();
            services.AddMigrationToolServicesForUnitTests();

            services.AddMigrationToolServices();
            services.AddMigrationToolServicesForClientFileSystem();

            return services.BuildServiceProvider();
        }
    }
}