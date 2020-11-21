using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CommandLine;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.WorkerService;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MigrationTools._EngineV1.Configuration;
using MigrationTools.Host.CommandLine;
using MigrationTools.Host.CustomDiagnostics;
using MigrationTools.Host.Services;
using MigrationTools.Options;
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
                 services.AddOptions();

                 Parser.Default.ParseArguments<InitOptions, ExecuteOptions>(args)
                     .WithParsed<InitOptions>(opts =>
                     {
                         services.AddSingleton(opts);
                         services.AddSingleton<ExecuteOptions>((p) => null);
                     })
                     .WithParsed<ExecuteOptions>(opts =>
                     {
                         services.AddSingleton(opts);
                         services.Configure<NetworkCredentialsOptions>(cred =>
                                         {
                                             cred.Source = new Credentials
                                                         {
                                                             Domain = opts.SourceDomain,
                                                             UserName = opts.SourceUserName,
                                                             Password = opts.SourcePassword
                                                         };
                                             cred.Target = new Credentials
                                                         {
                                                             Domain = opts.TargetDomain,
                                                             UserName = opts.TargetUserName,
                                                             Password = opts.TargetPassword
                                                         };
                                         });
                         services.AddSingleton<InitOptions>((p) => null);
                     })
                     .WithNotParsed(error =>
                     {
                         services.AddSingleton<InitOptions>((p) => null);
                         services.AddSingleton<ExecuteOptions>((p) => null);
                     });

                 // Sieralog
                 services.AddSingleton<LoggingLevelSwitch>(levelSwitch);
                 // Application Insights
                 services.AddApplicationInsightsTelemetryWorkerService(new ApplicationInsightsServiceOptions { InstrumentationKey = "2d666f84-b3fb-4dcf-9aad-65de038d2772" });

                 // Services
                 services.AddTransient<IDetectOnlineService, DetectOnlineService>();
                 services.AddTransient<IDetectVersionService, DetectVersionService>();

                 // Config
                 services.AddSingleton<IEngineConfigurationBuilder, EngineConfigurationBuilder>();
                 services.AddSingleton<EngineConfiguration>(sp =>
                 {
                     var executeOptions = sp.GetRequiredService<ExecuteOptions>();
                     var builder = sp.GetRequiredService<IEngineConfigurationBuilder>();
                     var logger = sp.GetServices<ILoggerFactory>().First().CreateLogger<EngineConfiguration>();

                     if (!File.Exists(executeOptions.ConfigFile))
                     {
                         logger.LogInformation("The config file {ConfigFile} does not exist, nor does the default 'configuration.json'. Use '{ExecutableName}.exe init' to create a configuration file first", executeOptions.ConfigFile, Assembly.GetEntryAssembly().GetName().Name);
                         throw new ArgumentException("missing configfile");
                     }
                     logger.LogInformation("Config Found, creating engine host");
                     return builder.BuildFromFile(executeOptions.ConfigFile);
                 });

                 /// Add Old v1Bits
                 services.AddMigrationToolServicesLegacy();
                 // New v2Bits
                 services.AddMigrationToolServices();

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