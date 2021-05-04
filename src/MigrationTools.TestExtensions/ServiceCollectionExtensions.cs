using Microsoft.ApplicationInsights.WorkerService;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;

namespace MigrationTools.TestExtensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddMigrationToolServicesForUnitTests(this IServiceCollection context)
        {
            // Add Fake AI Endpoint
            var aiOptions = new ApplicationInsightsServiceOptions
            {
                EndpointAddress = "http://localhost:8888/v2/track"
            };
            context.AddApplicationInsightsTelemetryWorkerService(aiOptions);
            context.AddSingleton<ITelemetryLogger, TelemetryClientAdapter>();
            // Logging for Unit Tests
            var loggers = new LoggerConfiguration().MinimumLevel.Verbose().Enrich.FromLogContext();
            loggers.WriteTo.Logger(logger => logger
              .WriteTo.Debug(restrictedToMinimumLevel: LogEventLevel.Verbose));
            Log.Logger = loggers.CreateLogger();
            Log.Logger.Information("Logger is initialized");
            context.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true));
        }
    }
}
