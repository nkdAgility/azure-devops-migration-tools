using Microsoft.Extensions.DependencyInjection;
using MigrationTools._EngineV1.Containers;
using MigrationTools.EndpointEnrichers;
using MigrationTools.Endpoints;
using MigrationTools.Enrichers;
using MigrationTools.Processors;
using MigrationTools.TestExtensions;

namespace MigrationTools.Tests
{
    internal static class ServiceProviderHelper
    {
        internal static ServiceProvider GetWorkItemMigrationProcessor()
        {
            var services = new ServiceCollection();
            services.AddMigrationToolServicesForUnitTests();
            services.AddMigrationToolServices();

            // Containers
            services.AddSingleton<ProcessorContainer>();
            services.AddSingleton<TypeDefinitionMapContainer>();
            services.AddSingleton<GitRepoMapContainer>();
            services.AddSingleton<FieldMapContainer>();
            services.AddSingleton<ChangeSetMappingContainer>();

            services.AddSingleton<ITelemetryLogger, TelemetryClientAdapter>();

            // Processors
            services.AddSingleton<WorkItemTrackingProcessor>();
            services.AddTransient<ProcessorEnricherContainer>();

            //Endpoints
            services.AddTransient<InMemoryWorkItemEndpoint>();
            services.AddTransient<EndpointEnricherContainer>();
            AddEndpoint(services, "Source");
            AddEndpoint(services, "Target");

            return services.BuildServiceProvider();
        }

        private static void AddEndpoint(IServiceCollection services, string name)
        {
            services.AddEndpoint(name, (provider) =>
            {
                var options = new InMemoryWorkItemEndpointOptions();
                var endpoint = provider.GetRequiredService<InMemoryWorkItemEndpoint>();
                endpoint.Configure(options);
                return endpoint;
            });
        }
    }
}