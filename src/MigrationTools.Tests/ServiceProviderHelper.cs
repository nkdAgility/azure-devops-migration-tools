using Microsoft.Extensions.DependencyInjection;
using MigrationTools._EngineV1.Containers;
using MigrationTools.Endpoints;
using MigrationTools.EndPoints;
using MigrationTools.Enrichers;
using MigrationTools.Processors;

namespace MigrationTools.Tests
{
    internal static class ServiceProviderHelper
    {
        internal static ServiceProvider GetWorkItemMigrationProcessor()
        {
            var services = new ServiceCollection();
            services.AddMigrationToolServicesForUnitTests();

            // Containers
            services.AddSingleton<ProcessorContainer>();
            services.AddSingleton<TypeDefinitionMapContainer>();
            services.AddSingleton<GitRepoMapContainer>();
            services.AddSingleton<FieldMapContainer>();
            services.AddSingleton<ChangeSetMappingContainer>();

            services.AddSingleton<ITelemetryLogger, TelemetryClientAdapter>();

            // Processors
            services.AddSingleton<WorkItemMigrationProcessor>();
            services.AddTransient<ProcessorEnricherContainer>();
            services.AddTransient<EndpointContainer>();

            //Endpoints
            services.AddTransient<InMemoryWorkItemEndpoint>();
            services.AddTransient<EndpointEnricherContainer>();

            return services.BuildServiceProvider();
        }
    }
}