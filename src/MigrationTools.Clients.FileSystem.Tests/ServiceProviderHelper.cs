using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using MigrationTools.Processors;

namespace MigrationTools.Tests
{
    internal static class ServiceProviderHelper
    {
        internal static ServiceProvider GetWorkItemMigrationProcessor()
        {
            var logger = new NullLogger<WorkItemMigrationProcessor>();
            var services = new ServiceCollection();
            services.AddApplicationInsightsTelemetryWorkerService();
            services.AddSingleton<ITelemetryLogger, TelemetryClientAdapter>();

            services.AddMigrationToolServices();
            services.AddMigrationToolServicesForClientFileSystem();

            return services.BuildServiceProvider();
        }
    }
}