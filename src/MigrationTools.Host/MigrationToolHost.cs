using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using CommandLine;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.WorkerService;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MigrationTools.CommandLine;
using MigrationTools.Configuration;
using MigrationTools.Engine.Containers;
using MigrationTools.Enrichers;

using MigrationTools.Host.CustomDiagnostics;
using MigrationTools.Host.Services;
using MigrationTools.Processors;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace MigrationTools.Host
{
    public static class MigrationToolHost
    {
        public static IHostBuilder CreateDefaultBuilder(string[] args)
        {
            var levelSwitch = new LoggingLevelSwitch(LogEventLevel.Verbose);
            var hostBuilder = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args)
             .UseSerilog((hostingContext, services, loggerConfiguration) =>
             {
                 string logsPath = CreateLogsPath();
                 var logPath = Path.Combine(logsPath, "migration.log");
                 loggerConfiguration
                     .MinimumLevel.ControlledBy(levelSwitch)
                     .ReadFrom.Configuration(hostingContext.Configuration)
                     .Enrich.FromLogContext()
                     .Enrich.WithMachineName()
                     .Enrich.WithProcessId()
                     .WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Debug, theme: AnsiConsoleTheme.Code)
                     .WriteTo.ApplicationInsights(services.GetService<TelemetryClient>(), new CustomConverter(), LogEventLevel.Error)
                     .WriteTo.File(logPath, LogEventLevel.Verbose);
             })
             .ConfigureLogging((context, logBuilder) =>
             {
             })
             .ConfigureServices((context, services) =>
             {
                 Parser.Default.ParseArguments<InitOptions, ExecuteOptions>(args)
                     .WithParsed<InitOptions>(opts =>
                     {
                         services.AddSingleton(opts);
                         services.AddSingleton<ExecuteOptions>((p) => null);
                     })
                     .WithParsed<ExecuteOptions>(opts =>
                     {
                         services.AddSingleton(opts);
                         services.AddSingleton<InitOptions>((p) => null);
                     })
                     .WithNotParsed(error =>
                     {
                         services.AddSingleton<InitOptions>((p) => null);
                         services.AddSingleton<ExecuteOptions>((p) => null);
                     });
                 services.AddOptions();
                 // Sieralog
                 services.AddSingleton<LoggingLevelSwitch>(levelSwitch);
                 // Application Insights
                 services.AddApplicationInsightsTelemetryWorkerService(new ApplicationInsightsServiceOptions { InstrumentationKey = "2d666f84-b3fb-4dcf-9aad-65de038d2772" });

                 // Services
                 services.AddTransient<IDetectOnlineService, DetectOnlineService>();
                 services.AddTransient<IDetectVersionService, DetectVersionService>();
                 services.AddSingleton<ITelemetryLogger, TelemetryClientAdapter>();

                 // Config
                 services.AddSingleton<IEngineConfigurationBuilder, EngineConfigurationBuilder>();
                 services.AddSingleton<EngineConfiguration, EngineConfigurationWrapper>();

                 // New v2Bits
                 services.AddMigrationToolEndpointEnrichers();
                 services.AddMigrationToolProcessorEnrichers();
                 services.AddMigrationToolProcessors();

                 //Engine
                 services.AddSingleton<FieldMapContainer>();
                 services.AddSingleton<ProcessorContainer>();
                 services.AddSingleton<TypeDefinitionMapContainer>();
                 services.AddSingleton<GitRepoMapContainer>();
                 services.AddSingleton<ChangeSetMappingContainer>();
                 services.AddSingleton<IMigrationEngine, MigrationEngine>();

                 // Host Services
                 services.AddTransient<IStartupService, StartupService>();
                 services.AddHostedService<InitHostedService>();
                 services.AddHostedService<ExecuteHostedService>();
             })
             .UseConsoleLifetime();
            return hostBuilder;
        }

        public static async Task RunMigrationTools(this IHostBuilder hostBuilder, string[] args)
        {
            var host = hostBuilder.Build();
            var startupService = host.InitializeMigrationSetup(args);
            if (startupService == null)
            {
                return;
            }
            await host.RunAsync();
            startupService.RunExitLogic();
        }

        private static string CreateLogsPath()
        {
            string exportPath;
            string assPath = Assembly.GetEntryAssembly().Location;
            exportPath = Path.Combine(Path.GetDirectoryName(assPath), "logs", DateTime.Now.ToString("yyyyMMddHHmmss"));
            if (!Directory.Exists(exportPath))
            {
                Directory.CreateDirectory(exportPath);
            }

            return exportPath;
        }
    }
}