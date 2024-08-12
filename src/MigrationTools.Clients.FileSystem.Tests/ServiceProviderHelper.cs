using Microsoft.Extensions.DependencyInjection;
using MigrationTools.Services;
using MigrationTools.TestExtensions;
using MigrationTools.Helpers.Tests;

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

            services.AddSingleton<IMigrationToolVersionInfo, FakeMigrationToolVersionInfo>();
            services.AddSingleton<IMigrationToolVersion, FakeMigrationToolVersion>();

            return services.BuildServiceProvider();
        }
    }
}