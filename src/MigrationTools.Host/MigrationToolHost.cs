using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MigrationTools.CommandLine;
using MigrationTools;
using MigrationTools.Configuration;
using MigrationTools.Engine.Containers;
using MigrationTools.CustomDiagnostics;
using MigrationTools.Services;
using Serilog;

namespace MigrationTools.Host
{
    public static class MigrationToolHost
    {
        public static IHostBuilder CreateDefaultBuilder(string[] args)
        {
            var hostBuilder = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args)
                .UseSerilog((hostingContext, services, loggerConfiguration) =>
                {
                    /////////////////////////////////////////////////////////
                    Trace.Listeners.Add(new TextWriterTraceListener(Console.Out)); // TODO: Remove once Trace replaced with log
                    var oldlogPath = Path.Combine(CreateLogsPath(), "old-migration.log"); // TODO: Remove once Trace replaced with log
                    Trace.Listeners.Add(new TextWriterTraceListener(oldlogPath, "myListener")); // TODO: Remove once Trace replaced with log
                    ///////////////////////////////////////////////////////////////////////////
                    string logsPath = CreateLogsPath();
                    var logPath = Path.Combine(logsPath, "migration.log");
                    loggerConfiguration
                        .ReadFrom.Configuration(hostingContext.Configuration)
                        .Enrich.FromLogContext()
                        .Enrich.WithMachineName()
                        .Enrich.WithProcessId()
                        .WriteTo.Console()
                        .WriteTo.ApplicationInsights(services.GetService<ITelemetryLogger>().Configuration, new CustomConverter(), Serilog.Events.LogEventLevel.Error)
                        .WriteTo.File(logPath);
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
                    // Services
                    services.AddTransient<IDetectOnlineService, DetectOnlineService>();
                    services.AddTransient<IDetectVersionService, DetectVersionService>();
                    services.AddSingleton<ITelemetryLogger, TelemetryClientAdapter>();
                    // Config
                    services.AddSingleton<IEngineConfigurationBuilder, EngineConfigurationBuilder>();
                    services.AddSingleton<EngineConfiguration, EngineConfigurationWrapper>();
                    //Engine
                    services.AddSingleton<FieldMapContainer>();
                    services.AddSingleton<ProcessorContainer>();
                    services.AddSingleton<TypeDefinitionMapContainer>();
                    services.AddSingleton<GitRepoMapContainer>();
                    services.AddSingleton<ChangeSetMappingContainer>();
                    services.AddSingleton<IMigrationEngine, MigrationEngine>();
                    // Host Services
                    services.AddTransient<IStartupService, StartupService>();

                    if (args.Any(o => o.Equals("init", StringComparison.InvariantCultureIgnoreCase)))
                    {
                        services.AddHostedService<InitHostedService>();
                    }
                    else
                    {
                        services.AddHostedService<ExecuteHostedService>();
                    }
                })
                .UseConsoleLifetime();
            return hostBuilder;
        }

        private static string CreateLogsPath()
        {
            string exportPath;
            string assPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            exportPath = Path.Combine(Path.GetDirectoryName(assPath), "logs", DateTime.Now.ToString("yyyyMMddHHmmss"));
            if (!Directory.Exists(exportPath))
            {
                Directory.CreateDirectory(exportPath);
            }

            return exportPath;
        }
    }
}
