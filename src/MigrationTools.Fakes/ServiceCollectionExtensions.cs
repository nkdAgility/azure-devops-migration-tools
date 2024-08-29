﻿using Microsoft.ApplicationInsights.WorkerService;
using Microsoft.Extensions.DependencyInjection;
using MigrationTools.Services;
using MigrationTools.Services.Shadows;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.InMemory;

namespace MigrationTools.Shadows
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
              .WriteTo.Debug(restrictedToMinimumLevel: LogEventLevel.Verbose))
              .WriteTo.InMemory();

            Log.Logger = loggers.CreateLogger();
            Log.Logger.Information("Logger is initialized");
            context.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true));
            // Add Telemitery Adapter
            context.AddSingleton<ITelemetryLogger, TelemetryLoggerFake>();
            context.AddSingleton<IMigrationToolVersion, FakeMigrationToolVersion>();
        }
    }
}
