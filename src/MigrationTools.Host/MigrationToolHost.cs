using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.WorkerService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MigrationTools._EngineV1.Configuration;
using MigrationTools.Host.CustomDiagnostics;
using MigrationTools.Host.Services;
using MigrationTools.Options;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using Spectre.Console.Cli.Extensions.DependencyInjection;
using Spectre.Console.Cli;
using Serilog.Filters;
using MigrationTools.Host.Commands;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace MigrationTools.Host
{
    public static class MigrationToolHost
    {
        static int logs = 1;

        public static IHostBuilder CreateDefaultBuilder(string[] args)
        {
           var configFile =  CommandSettingsBase.ForceGetConfigFile(args);

            var hostBuilder = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args);

            hostBuilder.UseSerilog((hostingContext, services, loggerConfiguration) =>
            {
                string outputTemplate = "[{Timestamp:HH:mm:ss} {Level:u3}] [" + DetectVersionService2.GetRunningVersion().versionString + "] {Message:lj}{NewLine}{Exception}"; // {SourceContext}
                string logsPath = CreateLogsPath();
                var logPath = Path.Combine(logsPath, $"migration{logs}.log");

                var logLevel = hostingContext.Configuration.GetValue<LogEventLevel>("LogLevel");
                var levelSwitch = new LoggingLevelSwitch(logLevel);
                loggerConfiguration
                    .MinimumLevel.ControlledBy(levelSwitch)
                    .ReadFrom.Configuration(hostingContext.Configuration)
                    .Enrich.FromLogContext()
                    .Enrich.WithMachineName()
                    .Enrich.WithProcessId()
                    .WriteTo.File(logPath, LogEventLevel.Verbose, outputTemplate)
                    .WriteTo.Logger(lc => lc
                        .Filter.ByExcluding(Matching.FromSource("Microsoft"))
                        //.Filter.ByExcluding(Matching.FromSource("MigrationTools.Host.StartupService"))
                        .WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Debug, theme: AnsiConsoleTheme.Code, outputTemplate: outputTemplate))
                    .WriteTo.Logger(lc => lc
                        .Filter.ByExcluding(Matching.FromSource("Microsoft"))
                        .WriteTo.ApplicationInsights(services.GetService<TelemetryClient>(), new CustomConverter(), LogEventLevel.Error));
                logs++;
            });

            hostBuilder.ConfigureLogging((context, logBuilder) =>
             {
             })
            .ConfigureAppConfiguration(builder =>
            {
                    builder.AddJsonFile(configFile);
            });

            hostBuilder.ConfigureServices((context, services) =>
             {
                 services.AddOptions();
                 services.Configure<EngineConfiguration>((config) =>
                 {
                     var sp = services.BuildServiceProvider();
                     var logger = sp.GetService<ILoggerFactory>().CreateLogger<EngineConfiguration>();
                     if (!File.Exists(configFile))
                     {
                         logger.LogInformation("The config file {ConfigFile} does not exist, nor does the default 'configuration.json'. Use '{ExecutableName}.exe init' to create a configuration file first", configFile, Assembly.GetEntryAssembly().GetName().Name);
                         throw new ArgumentException("missing configfile");
                     }
                     logger.LogInformation("Config Found, creating engine host");
                     var reader = sp.GetRequiredService<IEngineConfigurationReader>();
                     var parsed = reader.BuildFromFile(configFile);
                     config.ChangeSetMappingFile = parsed.ChangeSetMappingFile;
                     config.FieldMaps = parsed.FieldMaps;
                     config.GitRepoMapping = parsed.GitRepoMapping;
                     config.CommonEnrichersConfig = parsed.CommonEnrichersConfig;
                     config.Processors = parsed.Processors;
                     config.Source = parsed.Source;
                     config.Target = parsed.Target;
                     config.Version = parsed.Version;
                     config.workaroundForQuerySOAPBugEnabled = parsed.workaroundForQuerySOAPBugEnabled;
                     config.WorkItemTypeDefinition = parsed.WorkItemTypeDefinition;
                 });


                 // Application Insights
                 ApplicationInsightsServiceOptions aiso = new ApplicationInsightsServiceOptions();
                 aiso.ApplicationVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                 aiso.ConnectionString = "InstrumentationKey=2d666f84-b3fb-4dcf-9aad-65de038d2772;IngestionEndpoint=https://northeurope-0.in.applicationinsights.azure.com/;LiveEndpoint=https://northeurope.livediagnostics.monitor.azure.com/;ApplicationId=9146fe72-5c18-48d7-a0f2-8fb891ef1277";
                 //# if DEBUG
                 //aiso.DeveloperMode = true;
                 //#endif
                 services.AddApplicationInsightsTelemetryWorkerService(aiso);

                 // Services
                 services.AddTransient<IDetectOnlineService, DetectOnlineService>();
                 //services.AddTransient<IDetectVersionService, DetectVersionService>();
                 services.AddTransient<IDetectVersionService2, DetectVersionService2>();

                 // Config
                 services.AddSingleton<IEngineConfigurationBuilder, EngineConfigurationBuilder>();
                 services.AddTransient((provider) => provider.GetRequiredService<IEngineConfigurationBuilder>() as IEngineConfigurationReader);
                 services.AddTransient((provider) => provider.GetRequiredService<IEngineConfigurationBuilder>() as ISettingsWriter);

                 // Add Old v1Bits
                 services.AddMigrationToolServicesLegacy();
                 // New v2Bits
                 services.AddMigrationToolServices();

                 // Host Services
                 services.AddTransient<IStartupService, StartupService>();

             });

            hostBuilder.ConfigureServices((context, services) =>
            {
                using var registrar = new DependencyInjectionRegistrar(services);
                var app = new CommandApp(registrar);
                app.Configure(config =>
                {
                    config.PropagateExceptions();
                    config.AddCommand<Commands.ExecuteMigrationCommand>("execute");
                    config.AddCommand<Commands.InitMigrationCommand>("init");

                });
                services.AddSingleton<ICommandApp>(app);
            });

            hostBuilder.ConfigureServices((context, services) =>
            {
                services.AddHostedService<MigrationService>();
            });

            hostBuilder.UseConsoleLifetime();



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


            // Disanle telemitery from options
            //bool DisableTelemetry = false;
            //Serilog.ILogger logger = host.Services.GetService<Serilog.ILogger>();
            //if (executeOptions is not null && bool.TryParse(executeOptions.DisableTelemetry, out DisableTelemetry))
            //{
            //    TelemetryConfiguration ai = host.Services.GetService<TelemetryConfiguration>();
            //    ai.DisableTelemetry = DisableTelemetry;
            //}
            //logger.Information("Telemetry: {status}", !DisableTelemetry);

            await host.RunAsync();
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