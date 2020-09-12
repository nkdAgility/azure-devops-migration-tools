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

namespace MigrationTools.ConsoleUI
{
    class Program
    {
        static DateTime _startTime = DateTime.Now;
        static Stopwatch _mainTimer = new Stopwatch();
        static IHost host;

        static int Main(string[] args)
        {
            _mainTimer.Start();
            var InstrumentationKey = "4b9bb17b-c7ee-43e5-b220-ec6db2c33373";
            var sessionID = Guid.NewGuid();
            var ApplicationVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;

            var builder = new ConfigurationBuilder();
            BuildAppConfig(builder);

            var aiServiceOptions = new ApplicationInsightsServiceOptions();
            aiServiceOptions.InstrumentationKey = InstrumentationKey;
            aiServiceOptions.ApplicationVersion = ApplicationVersion.ToString();

            var telemetryConfiguration = TelemetryConfiguration.CreateDefault();
            telemetryConfiguration.InstrumentationKey = InstrumentationKey;

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(builder.Build())
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithProcessId()
                .WriteTo.Console()
                .WriteTo.ApplicationInsights(telemetryConfiguration, new CustomConverter())
                .WriteTo.File("log/migrate-core.log")
                .CreateLogger();

            LogContext.PushProperty("SessionID", sessionID);
            Log.Information("Application Starting");
            AsciiLogo(ApplicationVersion);
            Log.Information("Telemetry Note: We use Application Insights to collect telemetry on performance & feature usage for the tools to help our developers target features. This data is tied to a session ID that is generated and shown in the logs. This can help with debugging.");
            Log.Information("Start Time: {StartTime}", _startTime.ToUniversalTime().ToLocalTime());
            Log.Information("Running with args: {@Args}", args);
            Log.Information("OSVersion: {OSVersion}", Environment.OSVersion.ToString());
            Log.Information("Version: {CurrentVersion}", ApplicationVersion);
            Log.Information("userID: {UserId}", System.Security.Principal.WindowsIdentity.GetCurrent().Name);
            ///////////////////////////////////////////////////////
            /// Setup Host
            host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton<IDetectOnlineService, DetectOnlineService>();
                    services.AddSingleton<IDetectVersionService, DetectVersionService>();
                    services.AddApplicationInsightsTelemetryWorkerService(aiServiceOptions);
                    services.AddSingleton<IEngineConfigurationBuilder, EngineConfigurationBuilder>();
                    services.AddSingleton<MigrationEngine>();
                    services.AddTransient<IWorkItemSink, WorkItemSink>();
                })
                .UseSerilog()
                .Build();
           var tc = SetupTelemetry();
            var chk = CheckVersion(ApplicationVersion);
            if (chk != 0)
            {
                return chk;
            }
            //////////////////////////////////////////////////
            /// Setup Command Line
            int result = (int)Parser.Default.ParseArguments<InitOptions, ExecuteOptions>(args).MapResult(
                (InitOptions opts) => RunInitAndReturnExitCode(opts, tc),
                (ExecuteOptions opts) => RunExecuteAndReturnExitCode(opts, tc),
                errs => 1);
            ///////////////////////////////////////////////////////
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
            return result;
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
            Log.Information("Config Loaded, creating engine");
            var me = host.Services.GetRequiredService<MigrationEngine>();

            //VssCredentials sourceCredentials = null;
            //VssCredentials targetCredentials = null;
            //if (!string.IsNullOrWhiteSpace(opts.SourceUserName) && !string.IsNullOrWhiteSpace(opts.SourcePassword))
            //    sourceCredentials = new VssCredentials(new Microsoft.VisualStudio.Services.Common.WindowsCredential(new NetworkCredential(opts.SourceUserName, opts.SourcePassword, opts.SourceDomain)));

            //if (!string.IsNullOrWhiteSpace(opts.TargetUserName) && !string.IsNullOrWhiteSpace(opts.TargetPassword))
            //    targetCredentials = new VssCredentials(new Microsoft.VisualStudio.Services.Common.WindowsCredential(new NetworkCredential(opts.TargetUserName, opts.TargetPassword, opts.TargetDomain)));

            //MigrationEngine me;
            //if (sourceCredentials == null && targetCredentials == null)
            //    me = new MigrationEngine(ec);
            //else
            //    me = new MigrationEngine(ec, sourceCredentials, targetCredentials);

            if (!string.IsNullOrWhiteSpace(opts.ChangeSetMappingFile))
            {
                IChangeSetMappingProvider csmp = new ChangeSetMappingProvider(opts.ChangeSetMappingFile);
                //csmp.ImportMappings(me.ChangeSetMapping);
            }

            //Console.Title = $"Azure DevOps Migration Tools: {System.IO.Path.GetFileName(opts.ConfigFile)} - {System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(3)} - {ec.Source.Project} - {ec.Target.Project}";
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

        private static TelemetryClient SetupTelemetry()
        {
            var tc = host.Services.GetRequiredService<TelemetryClient>();
            tc.Context.User.Id = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            tc.Context.Session.Id = Guid.NewGuid().ToString();
            tc.Context.Device.OperatingSystem = Environment.OSVersion.ToString();
            tc.Context.Component.Version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            return tc;
        }

        private static int CheckVersion(Version ApplicationVersion)
        {
            var doService = ActivatorUtilities.GetServiceOrCreateInstance<IDetectOnlineService>(host.Services);
            if (doService.IsOnline())
            {
                var dvService = ActivatorUtilities.GetServiceOrCreateInstance<IDetectVersionService>(host.Services);
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
            return 0;
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
