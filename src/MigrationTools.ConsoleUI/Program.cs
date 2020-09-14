using MigrationTools.CommandLine;
using MigrationTools.CustomDiagnostics;
using MigrationTools.Services;
using CommandLine;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.WorkerService;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Context;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using MigrationTools.Core.Configuration;
using Newtonsoft.Json;
using MigrationTools.Core.Configuration.FieldMap;
using MigrationTools.Sinks.AzureDevOps;
using MigrationTools.Core.Sinks;
using Microsoft.ApplicationInsights.Extensibility.PerfCounterCollector;

namespace MigrationTools.ConsoleUI
{
    class Program
    {
        static DateTime _startTime = DateTime.Now;
        static Stopwatch _mainTimer = new Stopwatch();

        static int Main(string[] args)
        {
            _mainTimer.Start();
            var ApplicationVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
          
            var builder = new ConfigurationBuilder();
            BuildAppConfig(builder);

            var telemetryClient = GetTelemiteryClient();
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Build())
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithProcessId()
                .WriteTo.Console()
                .WriteTo.ApplicationInsights(telemetryClient, new CustomConverter())
                .WriteTo.File("log/migrate-core.log")
                .CreateLogger();

            Log.Information("Application Starting");
            AsciiLogo(ApplicationVersion);
            Log.Information("Telemetry Note: We use Application Insights to collect telemetry on performance & feature usage for the tools to help our developers target features. This data is tied to a session ID that is generated and shown in the logs. This can help with debugging.");
            Log.Information("Start Time: {StartTime}", _startTime.ToUniversalTime().ToLocalTime());
            Log.Information("Running with args: {@Args}", args);
            Log.Information("OSVersion: {OSVersion}", Environment.OSVersion.ToString());
            Log.Information("Version: {CurrentVersion}", ApplicationVersion);
            Log.Information("userID: {UserId}", System.Security.Principal.WindowsIdentity.GetCurrent().Name);

            var doService = new DetectOnlineService(telemetryClient);

            if (doService.IsOnline())
            {
                var dvService = new DetectVersionService(telemetryClient);
                Version latestVersion = dvService.GetLatestVersion();
                Log.Information("Latest version detected as {Version_Latest}", latestVersion);
                if (latestVersion > ApplicationVersion)
                {
                    Log.Warning("You are currently running version {Version_Current} and a newer version ({Version_Latest}) is available. You should upgrade now using Chocolatey command 'choco upgrade vsts-sync-migrator' from the command line.", ApplicationVersion, latestVersion);
#if !DEBUG
                    Console.WriteLine("Do you want to continue? (y/n)");
                    if (Console.ReadKey().Key != ConsoleKey.Y)
                    {
                        Log.Warning("User aborted to update version");
                        return 2;
                    }
#endif
                }
            }

            int result = (int)Parser.Default.ParseArguments<InitOptions, ExecuteOptions>(args).MapResult(
                (InitOptions opts) => RunInitAndReturnExitCode(opts, telemetryClient),
                (ExecuteOptions opts) => RunExecuteAndReturnExitCode(opts, telemetryClient),
                errs => 1);

            return result;
        }

        private static void NewMethod(TelemetryClient tc)
        {
            Log.Information("Application Ending");
            _mainTimer.Stop();
            tc.TrackEvent("ApplicationEnd", null,
                new Dictionary<string, double> {
                        { "Application_Elapsed", _mainTimer.ElapsedMilliseconds }
                });
            if (tc != null)
            {
                tc.Flush();
            }
            Log.Information("The application ran in {Application_Elapsed} and finished at {Application_EndTime}", _mainTimer.Elapsed.ToString("c"), DateTime.Now.ToUniversalTime().ToLocalTime());
            Log.CloseAndFlush();
            System.Threading.Thread.Sleep(1000);
        }

        private static int RunExecuteAndReturnExitCode(ExecuteOptions opts, TelemetryClient tc)
        {
            tc.TrackEvent("ExecuteCommand");

            if (opts.ConfigFile == string.Empty)
            {
                opts.ConfigFile = "configuration.json";
            }

            if (!File.Exists(opts.ConfigFile))
            {
                Log.Information("The config file {ConfigFile} does not exist, nor does the default 'configuration.json'. Use '{ExecutableName}.exe init' to create a configuration file first", opts.ConfigFile, System.Reflection.Assembly.GetExecutingAssembly().GetName().Name);
                return 1;
            }
            Log.Information("Config Found, creating engine host");
           var config = new EngineConfigurationBuilder().BuildFromFile();
            // Setup Host
            var host = Host.CreateDefaultBuilder()
                .ConfigureHostConfiguration(configHost =>
                {
                    configHost.SetBasePath(Directory.GetCurrentDirectory());
                    configHost.AddJsonFile("appsettings.json", optional: true);
                    configHost.AddEnvironmentVariables();
                })
                .ConfigureAppConfiguration((hostContext, configApp) =>
                {
                    configApp.SetBasePath(Directory.GetCurrentDirectory());
                    configApp.AddJsonFile(opts.ConfigFile, optional: false);
                    configApp.AddJsonFile(
                        $"{opts.ConfigFile}.{hostContext.HostingEnvironment.EnvironmentName}.json",
                        optional: true);
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddOptions();
                    services.AddSingleton<IDetectOnlineService, DetectOnlineService>();
                    services.AddSingleton<IDetectVersionService, DetectVersionService>();
                    services.AddSingleton(tc);
                    services.AddSingleton<EngineConfiguration>(config);
                    services.AddSingleton<MigrationEngine>();
                    services.AzureDevOpsWorkerServices(config);                    
                })
                .UseConsoleLifetime()
                .UseSerilog()
                .Build();
            
            var me = host.Services.GetRequiredService<MigrationEngine>();
            Console.Title = $"Azure DevOps Migration Tools: {System.IO.Path.GetFileName(opts.ConfigFile)} - {System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(3)} - {config.Source.Project} - {config.Target.Project}";
            Log.Information("Engine created, running...");
            me.Run();
            Log.Information("Run complete...");
            return 0;
        }

        private static int RunInitAndReturnExitCode(InitOptions opts, TelemetryClient telemetryClient)
        {
            telemetryClient.TrackEvent("InitCommand");

            string configFile = opts.ConfigFile;
            if (string.IsNullOrEmpty(configFile))
            {
                configFile = "configuration.json";
            }
            Log.Information("ConfigFile: {configFile}", configFile);
            if (File.Exists(configFile))
            {
                Log.Information("Deleting old configuration.json reference file");
                File.Delete(configFile);
            }
            if (!File.Exists(configFile))
            {
                Log.Information("Populating config with {Options}", opts.Options.ToString());
                IEngineConfigurationBuilder cbuilder = new EngineConfigurationBuilder();
                EngineConfiguration config;
                switch (opts.Options)
                {
                    case OptionsMode.Full:
                        config = cbuilder.BuildDefault();
                        break;
                    case OptionsMode.WorkItemTracking:
                        config = cbuilder.BuildWorkItemMigration();
                        break;
                    default:
                        config = cbuilder.BuildDefault();
                        break;
                }

                string json = JsonConvert.SerializeObject(config, Formatting.Indented,
                    new FieldMapConfigJsonConverter(),
                    new ProcessorConfigJsonConverter());
                StreamWriter sw = new StreamWriter(configFile);
                sw.WriteLine(json);
                sw.Close();
                Log.Information("New configuration.json file has been created");
            }
            return 0;
        }

        private static TelemetryClient GetTelemiteryClient()
        {
            var telemetryConfiguration = TelemetryConfiguration.CreateDefault();
            telemetryConfiguration.InstrumentationKey = "4b9bb17b-c7ee-43e5-b220-ec6db2c33373";
            var tc = new TelemetryClient(telemetryConfiguration);
            tc.Context.User.Id = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            tc.Context.Session.Id = Guid.NewGuid().ToString();
            tc.Context.Device.OperatingSystem = Environment.OSVersion.ToString();
            tc.Context.Component.Version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            var perfCollectorModule = new PerformanceCollectorModule();
            perfCollectorModule.Counters.Add(new PerformanceCounterCollectionRequest(
              string.Format(@"\.NET CLR Memory({0})\# GC Handles", System.AppDomain.CurrentDomain.FriendlyName), "GC Handles"));
            perfCollectorModule.Initialize(telemetryConfiguration);
            return tc;
        }


        static void BuildAppConfig(IConfigurationBuilder builder)
        {
            builder.SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional:false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();
        }

        private static void AsciiLogo(Version thisVersion)
        {
            Log.Information("                                      &@&                                      ");
            Log.Information("                                   @@(((((@                                    ");
            Log.Information("                                  @(((((((((@                                  ");
            Log.Information("                                @(((((((((((((&                                ");
            Log.Information("                              ##((((((@ @((((((@@                              ");
            Log.Information("                             @((((((@     @((((((&                             ");
            Log.Information("                            @(((((#        @((((((@                            ");
            Log.Information("                           &(((((&           &(((((@                           ");
            Log.Information("                          @(((((&             &(((((@                          ");
            Log.Information("                          &(((((@#&@((.((&@@@(#(((((@                          ");
            Log.Information("                         #((((#..................#@((&                         ");
            Log.Information("                       &@(((((&......................(@                        ");
            Log.Information("                     @.(&((((&...&&        &@&..........&@                     ");
            Log.Information("                   @...@(((((@                   @#.......((                   ");
            Log.Information("                 &.....@(((((@                   @((@.......&                  ");
            Log.Information("                @......@(((((                    #((((&.......&                ");
            Log.Information("               #.....( &(((((         @@@        ((((((@@......@               ");
            Log.Information("              &.....@  @(((&@@#(((((((((((((((((#@(((((&  ......@              ");
            Log.Information("             @.....@  &@&((((((((((((((((((((((((@(((((@#  ......@             ");
            Log.Information("            @.....&@(((((((((((((((&&@@@@@(((((@((((#(((#@(....&               ");
            Log.Information("            @.....&((((((((&@@&                 @(((((@(((((((@...#            ");
            Log.Information("            &....((((((@@(((((@                &@(((((@&((((((((#&&            ");
            Log.Information("           @(....&((@    @(((((@               @(((((@    @(((((((##           ");
            Log.Information("         @(#(....&        &(((((@             @(((((&       &@(((((((&         ");
            Log.Information("       &@(((&.....        @((((((&           @(((((       &.(&((((((@          ");
            Log.Information("      @(((((@.....&        (((((@        &@(((((&         @....@((((((@        ");
            Log.Information("     @(((((#@.....(          &(((((@&     ##(((((&         @.....@@((((((@     ");
            Log.Information("   (&(((((@  &.....@&         @((((((@   @((((((@         @......   @(((((@    ");
            Log.Information("   &(((((@    @.....#&         @#((((((@((((((#          @......&    @(((((@   ");
            Log.Information("  @(((((@      &......&          @(((((((@#((@         &@......       @(((((@  ");
            Log.Information(" @(((((@        @......@&        @@@(((((((&@&        @......(         #(((((@ ");
            Log.Information(" #((((&           &.......@  &@&(((((@#((((((((@@& &@.......@          ((((&   ");
            Log.Information("&(((((@@           @(....&@#((((((((((@ @(((((((#@........@            &@(((((@");
            Log.Information("&(((((((((((((((((((((((((((((((((&@@@@@@@@@&...........@(((((((((((((((((((((@");
            Log.Information("@(((((((((((((((((((((((((((((&@(....................@#((((((((((((((((((((((#@");
            Log.Information("      @((((((((((((((&@&  &&...................@   @@#((((((((((((((#@@        ");
            Log.Information("                                                                               ");
            Log.Information("===============================================================================");
            Log.Information("===                       Azure DevOps Migration Tools  (REST EDITION)      ==");
            Log.Information($"===                                 v{thisVersion}                                ==");
            Log.Information("===============================================================================");
        }

    }
}
