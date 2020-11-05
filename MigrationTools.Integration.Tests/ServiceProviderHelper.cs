using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;

namespace MigrationTools.Tests
{
    internal static class ServiceProviderHelper
    {
        internal static ServiceProvider GetServicesV2()
        {
            var loggers = new LoggerConfiguration().MinimumLevel.Verbose().Enrich.FromLogContext();
            loggers.WriteTo.Logger(logger => logger
              .WriteTo.Debug(restrictedToMinimumLevel: LogEventLevel.Verbose));
            Log.Logger = loggers.CreateLogger();
            Log.Logger.Information("Logger is initialized");

            var services = new ServiceCollection();
            services.AddApplicationInsightsTelemetryWorkerService();
            services.AddSingleton<ITelemetryLogger, TelemetryClientAdapter>();
            services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true));
            /////////////////////////////////
            services.AddMigrationToolServices();
            services.AddMigrationToolServicesForClientInMemory();
            services.AddMigrationToolServicesForClientFileSystem();
            services.AddMigrationToolServicesForClientAzureDevOpsObjectModel();
            services.AddMigrationToolServicesForClientAzureDevopsRest();

            return services.BuildServiceProvider();
        }
    }
}