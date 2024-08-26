using Microsoft.Extensions.DependencyInjection;
using MigrationTools.Services;
using MigrationTools.TestExtensions;
using Microsoft.Extensions.Configuration;
using MigrationTools.Services.Shadows;

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
            services.AddMigrationToolServicesForClientFileSystem(configuration);

            services.AddSingleton<IMigrationToolVersionInfo, FakeMigrationToolVersionInfo>();
            services.AddSingleton<IMigrationToolVersion, FakeMigrationToolVersion>();

            return services.BuildServiceProvider();
        }
    }
}