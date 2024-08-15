using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MigrationTools._EngineV1.Containers;
using MigrationTools.EndpointEnrichers;
using MigrationTools.Endpoints;
using MigrationTools.Engine.Containers.Tests;
using MigrationTools.Enrichers;
using MigrationTools.Helpers.Tests;
using MigrationTools.ProcessorEnrichers.WorkItemProcessorEnrichers;
using MigrationTools.Processors;
using MigrationTools.Services;
using MigrationTools.TestExtensions;

namespace MigrationTools.Tests
{
    internal static class ServiceProviderHelper
    {
        internal static ServiceProvider GetWorkItemMigrationProcessor()
        {
            var configuration = new ConfigurationBuilder().Build();
            var services = new ServiceCollection();
            services.AddMigrationToolServicesForUnitTests();
            services.AddMigrationToolServices(configuration);

            // Containers
            services.AddSingleton<ProcessorContainer>();
            services.AddSingleton<GitRepoMapContainer>();

            services.AddSingleton<ITelemetryLogger, TelemetryClientAdapter>();

            // Processors
            services.AddSingleton<WorkItemTrackingProcessor>();
            services.AddTransient<ProcessorEnricherContainer>();

            // ProcessorEnrichers
            services.AddSingleton<StringManipulatorEnricher>();


            services.AddSingleton<IMigrationToolVersionInfo, FakeMigrationToolVersionInfo>();
            services.AddSingleton<IMigrationToolVersion, FakeMigrationToolVersion>();
            services.AddTransient<SimpleFieldMapMock>();

            return services.BuildServiceProvider();
        }

    }
}