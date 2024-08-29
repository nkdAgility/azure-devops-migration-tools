using System;
using System.IO;
using System.Reflection;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.WorkerService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MigrationTools._EngineV1.Configuration;
using MigrationTools.Host.CustomDiagnostics;
using MigrationTools.Host.Services;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using Spectre.Console.Cli;
using Serilog.Filters;
using MigrationTools.Host.Commands;
using MigrationTools.Services;
using Spectre.Console.Extensions.Hosting;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using static MigrationTools.ConfigurationExtensions;
using MigrationTools.Options;
using MigrationTools.Endpoints;
using MigrationTools.Endpoints.Infrastructure;
using Microsoft.Extensions.Options;
using System.Configuration;
using Spectre.Console;

namespace MigrationTools.Host
{



    public static class MigrationToolHost
    {
        static int logs = 1;
        private static bool LoggerHasBeenBuilt = false;

        public static IEnumerable<T> GetAll<T>(this IServiceProvider provider)
        {
            var site = typeof(ServiceProvider).GetProperty("CallSiteFactory", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(provider);
            var desc = site.GetType().GetField("_descriptors", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(site) as ServiceDescriptor[];
            return desc.Select(s => provider.GetRequiredService(s.ServiceType)).OfType<T>();
        }

        public static IHostBuilder CreateDefaultBuilder(string[] args, Action<IConfigurator> extraCommands = null)
        {
            var configFile = CommandSettingsBase.ForceGetConfigFile(args);
            var mtv = new MigrationToolVersion();

            string logsPath = CreateLogsPath();

            var hostBuilder = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args);

            var outputTemplate = "[{Timestamp:HH:mm:ss} {Level:u3}] [{versionString}] {Message:lj} {NewLine}{Exception}"; // 

            hostBuilder.UseSerilog((hostingContext, services, loggerConfiguration) =>
            {
                loggerConfiguration
                    .ReadFrom.Configuration(hostingContext.Configuration)
                    .Enrich.WithProperty("versionString", mtv.GetRunningVersion().versionString)
                    .Enrich.FromLogContext()
                    .Enrich.WithProcessId()
                    .WriteTo.ApplicationInsights(services.GetService<TelemetryConfiguration>(), new CustomConverter(), LogEventLevel.Error)
                    .WriteTo.File(Path.Combine(logsPath, $"migration.log"), LogEventLevel.Verbose, shared: true,outputTemplate: outputTemplate)
                    .WriteTo.File(new Serilog.Formatting.Json.JsonFormatter(), Path.Combine(logsPath, $"migration-errors.log"), LogEventLevel.Error, shared: true)
                    .WriteTo.Logger(lc => lc
                            .Filter.ByExcluding(Matching.FromSource("Microsoft.Hosting.Lifetime"))
                            .Filter.ByExcluding(Matching.FromSource("Microsoft.Extensions.Hosting.Internal.Host"))
                            .WriteTo.Console(theme: AnsiConsoleTheme.Code, outputTemplate: outputTemplate))
                    ;
                    
                LoggerHasBeenBuilt = true;
            });

            hostBuilder.ConfigureLogging((context, logBuilder) =>
             {
             })
            .ConfigureAppConfiguration(builder =>
            {
                if (!string.IsNullOrEmpty(configFile) && File.Exists(configFile))
                {
                    builder.AddJsonFile(configFile);
                }
                builder.AddEnvironmentVariables();
                builder.AddCommandLine(args);
            });

            hostBuilder.ConfigureServices((context, services) =>
            {

                    services.AddOptions();

                 // Application Insights
                 ApplicationInsightsServiceOptions aiso = new ApplicationInsightsServiceOptions();
                 aiso.ApplicationVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                 aiso.ConnectionString = "InstrumentationKey=2d666f84-b3fb-4dcf-9aad-65de038d2772;IngestionEndpoint=https://northeurope-0.in.applicationinsights.azure.com/;LiveEndpoint=https://northeurope.livediagnostics.monitor.azure.com/;ApplicationId=9146fe72-5c18-48d7-a0f2-8fb891ef1277";
                 //# if DEBUG
                 //aiso.DeveloperMode = true;
                 //#endif
                 services.AddApplicationInsightsTelemetryWorkerService(aiso);

                 //// Services
                 services.AddTransient<IDetectOnlineService, DetectOnlineService>();
                 //services.AddTransient<IDetectVersionService, DetectVersionService>();
                 services.AddTransient<IDetectVersionService2, DetectVersionService2>();

                 services.AddSingleton<IMigrationToolVersionInfo, MigrationToolVersionInfo>();
                 services.AddSingleton<IMigrationToolVersion, MigrationToolVersion>();

                 //// Add Old v1Bits
                 services.AddMigrationToolServicesLegacy();
                 //// New v2Bits
                 services.AddMigrationToolServices(context.Configuration, configFile);
             });

            hostBuilder.UseSpectreConsole(config =>
            {
                config.AddCommand<Commands.ExecuteMigrationCommand>("execute")
                            .WithDescription("Executes the enables processors specified in the configuration file.")
                            .WithExample("execute -config \"configuration.json\"")
                            .WithExample("execute -config \"configuration.json\" --skipVersionCheck ");
                config.AddCommand<Commands.InitMigrationCommand>("init")
                            .WithDescription("Creates an default configuration file")
                            .WithExample("init -options Basic")
                            .WithExample("init -options WorkItemTracking ")
                            .WithExample("init -options Reference ");
                config.AddCommand<Commands.UpgradeConfigCommand>("upgrade")
                           .WithDescription("Atempts to upgrade your config from the old version to the new one. For each object we will load the defaults, then apply your config. This will only bring accross valid settings. This is 'best effort' and you will need to check all the values as we have changed a lot!")
                           .WithExample("upgrade -config \"configuration.json\"");

                //config.AddCommand<Commands.MigrationConfigCommand>("config")
                //            .WithDescription("Creates or edits a configuration file")
                //           .WithExample("config -config \"configuration.json\"");

                extraCommands?.Invoke(config);
                config.PropagateExceptions();
            });
            hostBuilder.UseConsoleLifetime(configureOptions =>
            {
                configureOptions.SuppressStatusMessages = true;
            } );

   

            return hostBuilder;
        }

        static string logDate = DateTime.Now.ToString("yyyyMMddHHmmss");

        private static string CreateLogsPath()
        {
            string exportPath;
            string assPath = Assembly.GetEntryAssembly().Location;
            exportPath = Path.Combine(Path.GetDirectoryName(assPath), "logs", logDate);
            if (!Directory.Exists(exportPath))
            {
                Directory.CreateDirectory(exportPath);
            }

            return exportPath;
        }

    }
}