using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MigrationTools._EngineV1.Containers;
using MigrationTools.EndpointEnrichers;
using MigrationTools.Endpoints;
using MigrationTools.Engine.Containers.Tests;
using MigrationTools.Enrichers;
using MigrationTools.Processors;
using MigrationTools.Processors.Infrastructure.Shadows;
using MigrationTools.Services;
using MigrationTools.Services.Shadows;
using MigrationTools.Shadows;
using MigrationTools.Tools;
using MigrationTools.Tools.Shadows;

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
            services.AddTransient<MockSimpleFieldMap>();
            services.AddTransient<MockSimpleFieldMapOptions>();
            services.AddTransient<MockSimpleProcessor>();

            return services;
        }

    }
}