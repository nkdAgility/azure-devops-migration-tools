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

namespace MigrationTools.Host
{
    public static class MigrationToolHost
    {
        public static IHostBuilder CreateDefaultBuilder(string[] args)
        {
            var hostBuilder = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args);

            hostBuilder.UseSerilog((hostingContext, services, loggerConfiguration) =>
            {
                string outputTemplate = "[{Timestamp:HH:mm:ss} {Level:u3}] [" + GetVersionTextForLog() + "] {Message:lj}{NewLine}{Exception}";
                string logsPath = CreateLogsPath();
                var logPath = Path.Combine(logsPath, "migration.log");
                var logLevel = hostingContext.Configuration.GetValue<LogEventLevel>("LogLevel");
                var levelSwitch = new LoggingLevelSwitch(logLevel);
                loggerConfiguration
                    .MinimumLevel.ControlledBy(levelSwitch)
                    .ReadFrom.Configuration(hostingContext.Configuration)
                    .Enrich.FromLogContext()
                    .Enrich.WithMachineName()
                    .Enrich.WithProcessId()
                    .WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Debug, theme: AnsiConsoleTheme.Code, outputTemplate: outputTemplate)
                    .WriteTo.ApplicationInsights(services.GetService<TelemetryClient>(), new CustomConverter(), LogEventLevel.Error)
                    .WriteTo.File(logPath, LogEventLevel.Verbose, outputTemplate: outputTemplate);
            });

            hostBuilder.ConfigureLogging((context, logBuilder) =>
             {
             });
            //.ConfigureAppConfiguration(builder =>
            //{
            //    if (executeOptions is not null)
            //    {
            //        builder.AddJsonFile(executeOptions.ConfigFile);
            //    }
            //})

            hostBuilder.ConfigureServices((context, services) =>
             {
                 services.AddOptions();
                 services.Configure<EngineConfiguration>((config) =>
                 {
                     var sp = services.BuildServiceProvider();
                     var logger = sp.GetService<ILoggerFactory>().CreateLogger<EngineConfiguration>();
                     //if (!File.Exists(executeOptions.ConfigFile))
                     //{
                     //    logger.LogInformation("The config file {ConfigFile} does not exist, nor does the default 'configuration.json'. Use '{ExecutableName}.exe init' to create a configuration file first", executeOptions.ConfigFile, Assembly.GetEntryAssembly().GetName().Name);
                     //    throw new ArgumentException("missing configfile");
                     //}
                     //logger.LogInformation("Config Found, creating engine host");
                     //var reader = sp.GetRequiredService<IEngineConfigurationReader>();
                     //var parsed = reader.BuildFromFile(executeOptions.ConfigFile);
                     //config.ChangeSetMappingFile = parsed.ChangeSetMappingFile;
                     //config.FieldMaps = parsed.FieldMaps;
                     //config.GitRepoMapping = parsed.GitRepoMapping;
                     //config.CommonEnrichersConfig = parsed.CommonEnrichersConfig;
                     //config.Processors = parsed.Processors;
                     //config.Source = parsed.Source;
                     //config.Target = parsed.Target;
                     //config.Version = parsed.Version;
                     //config.workaroundForQuerySOAPBugEnabled = parsed.workaroundForQuerySOAPBugEnabled;
                     //config.WorkItemTypeDefinition = parsed.WorkItemTypeDefinition;
                 });

                 
                 // Application Insights
                 ApplicationInsightsServiceOptions aiso = new ApplicationInsightsServiceOptions();
                 aiso.ApplicationVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
                 aiso.ConnectionString = "InstrumentationKey=2d666f84-b3fb-4dcf-9aad-65de038d2772";
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
                    config.AddCommand<Commands.MigrationExecuteCommand>("execute");
                    config.AddCommand<Commands.MigrationInitCommand>("init");

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

        private static string GetVersionTextForLog()
        {
            Version runningVersion = DetectVersionService2.GetRunningVersion().version;
            string textVersion = "v" + DetectVersionService2.GetRunningVersion().version + "-" + DetectVersionService2.GetRunningVersion().PreReleaseLabel;
            return textVersion;
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