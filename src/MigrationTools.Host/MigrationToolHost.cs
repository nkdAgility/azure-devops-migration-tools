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
using Spectre.Console.Cli;
using Serilog.Filters;
using MigrationTools.Host.Commands;
using System.Diagnostics;
using System.Text.RegularExpressions;
using MigrationTools.Services;
using Spectre.Console.Extensions.Hosting;
using System.Configuration;
using NuGet.Protocol.Plugins;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using static System.Collections.Specialized.BitVector32;
using System.Text.Json;

namespace MigrationTools.Host
{



    public static class MigrationToolHost
    {
        static int logs = 1;
        private static bool LoggerHasBeenBuilt = false;

        public static IHostBuilder CreateDefaultBuilder(string[] args)
        {
            var configFile = CommandSettingsBase.ForceGetConfigFile(args);
            var mtv = new MigrationToolVersion();

            var hostBuilder = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args);
            hostBuilder.UseSerilog((hostingContext, services, loggerConfiguration) =>
            {
                string outputTemplate = "[{Timestamp:HH:mm:ss} {Level:u3}] [" + mtv.GetRunningVersion().versionString + "] {Message:lj}{NewLine}{Exception}"; // {SourceContext}
                string logsPath = CreateLogsPath();

                var logPath = Path.Combine(logsPath, $"migration-{logs}.log");

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
                        .Filter.ByExcluding(Matching.FromSource("Microsoft.Hosting.Lifetime"))
                        .Filter.ByExcluding(Matching.FromSource("Microsoft.Extensions.Hosting.Internal.Host"))
                        .WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Debug, theme: AnsiConsoleTheme.Code, outputTemplate: outputTemplate))
                    .WriteTo.Logger(lc => lc
                        .WriteTo.ApplicationInsights(services.GetService<TelemetryConfiguration>(), new CustomConverter(), LogEventLevel.Error));
                logs++;
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

                 services.AddOptions<EngineConfiguration>().Configure<IEngineConfigurationReader, ILogger<EngineConfiguration>, IConfiguration>(
                   (options, reader, logger, configuration) =>
                   {
                       if (!File.Exists(configFile))
                       {
                           logger.LogCritical("The config file {ConfigFile} does not exist, nor does the default 'configuration.json'. Use '{ExecutableName}.exe init' to create a configuration file first", configFile, Assembly.GetEntryAssembly().GetName().Name);
                           Environment.Exit(-1);
                       }
                       string configVersionString = configuration.GetValue<string>("MigrationTools:Version");
                       if (string.IsNullOrEmpty(configVersionString))
                       {
                           configVersionString = configuration.GetValue<string>("Version");
                       }
                       if (string.IsNullOrEmpty(configVersionString))
                       {
                           configVersionString = "0.0";
                       }
                       Version.TryParse(configVersionString, out Version configVersion);
                       logger.LogInformation("Config Found, creating engine host");
                       if (configVersion < Version.Parse("16.0"))
                       {
                           logger.LogCritical("Config is from an older version of the tooling and will need updated. For now we will load it, but at some point this ability will be removed.");
                           var parsed = reader.BuildFromFile(configFile); // TODO revert tp 
                           options.ChangeSetMappingFile = parsed.ChangeSetMappingFile;
                           options.FieldMaps = parsed.FieldMaps;
                           options.GitRepoMapping = parsed.GitRepoMapping;
                           options.CommonEnrichersConfig = parsed.CommonEnrichersConfig;
                           options.Processors = parsed.Processors;
                           options.Source = parsed.Source;
                           options.Target = parsed.Target;
                           options.Version = parsed.Version;
                           options.WorkItemTypeDefinition = parsed.WorkItemTypeDefinition;
                       } else
                       {
                           // This code Converts the new config format to the v1 and v2 runtme format.
                           options.Version = configuration.GetValue<string>("MigrationTools:Version");
                           options.ChangeSetMappingFile = configuration.GetValue<string>("MigrationTools:ChangeSetMappingFile");
                           //options.FieldMaps = configuration.GetSection("MigrationTools:FieldMaps").Get<IFieldMap[]>();
                           options.GitRepoMapping = configuration.GetValue<Dictionary<string, string>>("MigrationTools:MappingTools:WorkItemGitRepoMapping:WorkItemGitRepos");
                           options.CommonEnrichersConfig = configuration.GetSection("MigrationTools:CommonEnrichers")?.GetChildren()?.ToList().ConvertAll<Enrichers.IProcessorEnricherOptions>(x => x.GetMigrationOptionFromConfig<Enrichers.IProcessorEnricherOptions>());
                          options.Processors = configuration.GetSection("MigrationTools:Processors").GetChildren()?.ToList().ConvertAll<IProcessorConfig>(config =>
                          {
                              var processorTypeString = config.GetValue<string>("ProcessorType");
                              var processorType = GetProcessorFromTypeString(processorTypeString);
                              var obj = Activator.CreateInstance(processorType);
                              config.Bind(obj);
                              return (IProcessorConfig)obj;
                          });
                           options.Source = null;
                           options.Target = null;
                           options.Version = null;
                           options.WorkItemTypeDefinition = null;
                           Log.Information("CommonEnrichersConfig: {CommonEnrichersConfig}", options.CommonEnrichersConfig);
                       }








                   }

               );

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

                 services.AddSingleton<IMigrationToolVersionInfo, MigrationToolVersionInfo>();
                 services.AddSingleton<IMigrationToolVersion, MigrationToolVersion>();


                 // Config
                 services.AddSingleton<IEngineConfigurationBuilder, EngineConfigurationBuilder>();
                 services.AddTransient((provider) => provider.GetRequiredService<IEngineConfigurationBuilder>() as IEngineConfigurationReader);
                 services.AddTransient((provider) => provider.GetRequiredService<IEngineConfigurationBuilder>() as ISettingsWriter);

                 // Add Old v1Bits
                 services.AddMigrationToolServicesLegacy();
                 // New v2Bits
                 services.AddMigrationToolServices();
             });

            hostBuilder.UseSpectreConsole(config =>
            {
                config.AddCommand<Commands.ExecuteMigrationCommand>("execute");
                config.AddCommand<Commands.InitMigrationCommand>("init");
                config.PropagateExceptions();
            });
            hostBuilder.UseConsoleLifetime();



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

        private static Type GetProcessorFromTypeString(string processorType)
        {
            // Get all loaded assemblies in the current application domain

            // Get all types from each assembly
            IEnumerable<Type> prosserOptionTypes = GetTypesImplementingInterface<IProcessorConfig>();
            return prosserOptionTypes.SingleOrDefault(type => type.Name.StartsWith(processorType));
        }

        private static IEnumerable<Type> GetTypesImplementingInterface<TInterface>()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(ass => (ass.FullName.StartsWith("MigrationTools") || ass.FullName.StartsWith("VstsSyncMigrator")));
            var interfaceType = typeof(TInterface);
            return assemblies
                        .SelectMany(assembly => assembly.GetTypes())
                        .Where(type => interfaceType.IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract);
        }


    }
}