using System.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MigrationTools.Helpers.Tests;
using MigrationTools.Services;
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

            services.AddMigrationToolServices(configuration);
            services.AddMigrationToolServicesForClientInMemory();

            services.AddSingleton<IMigrationToolVersionInfo, FakeMigrationToolVersionInfo>();
            services.AddSingleton<IMigrationToolVersion, FakeMigrationToolVersion>();

            return services.BuildServiceProvider();
        }
    }
}