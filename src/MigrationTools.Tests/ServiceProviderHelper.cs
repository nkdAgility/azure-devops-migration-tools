using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MigrationTools._EngineV1.Containers;
using MigrationTools.EndpointEnrichers;
using MigrationTools.Endpoints;
using MigrationTools.Engine.Containers.Tests;
using MigrationTools.Enrichers;
using MigrationTools.Helpers.Tests;
using MigrationTools.Processors;
using MigrationTools.Services;
using MigrationTools.TestExtensions;
using MigrationTools.Tools;

namespace MigrationTools.Tests
{
    internal static class ServiceProviderHelper
    {
        internal static ServiceProvider GetWorkItemMigrationProcessor()
        {
            var configuration = new ConfigurationBuilder().Build();
            var services = GetServiceCollection();
            return services.BuildServiceProvider();
        }

        internal static ServiceCollection GetServiceCollection()
        {
            var configuration = new ConfigurationBuilder().Build();
            var services = new ServiceCollection();
            services.AddMigrationToolServicesForUnitTests();
            services.AddMigrationToolServices(configuration);


            services.AddSingleton<ITelemetryLogger, TelemetryClientAdapter>();

            // Processors
            services.AddSingleton<WorkItemTrackingProcessor>();
            services.AddTransient<ProcessorEnricherContainer>();

            // ProcessorEnrichers
            services.AddSingleton<StringManipulatorTool>();


            services.AddSingleton<IMigrationToolVersionInfo, FakeMigrationToolVersionInfo>();
            services.AddSingleton<IMigrationToolVersion, FakeMigrationToolVersion>();
            services.AddTransient<SimpleFieldMapMock>();
            services.AddTransient<SimpleProcessorMock>();

            return services;
        }

    }
}