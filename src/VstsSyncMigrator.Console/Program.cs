using MigrationTools.CommandLine;
using MigrationTools.Core.Configuration;
using MigrationTools.Core.Configuration.FieldMap;
using MigrationTools.Services;
using CommandLine;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.WorkerService;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.Services.Common;
using Newtonsoft.Json;
using NuGet;
using Serilog;
using Serilog.Context;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using VstsSyncMigrator.Commands;
using VstsSyncMigrator.Engine;

namespace VstsSyncMigrator.ConsoleApp
{
    public class Program
    {
        static DateTime startTime = DateTime.Now;
        static Stopwatch mainTimer = new Stopwatch();

        public static int Main(string[] args)
        {
            Telemetry.Current.TrackEvent("ApplicationStart");
            mainTimer.Start();
            string logsPath = CreateLogsPath();
            var logPath = Path.Combine(logsPath, "migration.log");
            var log = new LoggerConfiguration().Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.ApplicationInsights(Telemetry.Current, TelemetryConverter.Traces)
                .WriteTo.File(logPath, rollingInterval: RollingInterval.Infinite)
                .CreateLogger();
            Log.Logger = log;
            Log.Information("Writing log to {logPath}", logPath);
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            //////////////////////////////////////////////////
            /////////////////////////////////////////////////////////
            Trace.Listeners.Add(new TextWriterTraceListener(Console.Out)); // TODO: Remove once Trace replaced with log
            var oldlogPath = Path.Combine(logsPath, "old-migration.log"); // TODO: Remove once Trace replaced with log
            Trace.Listeners.Add(new TextWriterTraceListener(logPath, "myListener")); // TODO: Remove once Trace replaced with log
            ///////////////////////////////////////////////////////////////////////////
            //////////////////////////////////////////////////
            Version thisVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            AsciiLogo(thisVersion);
            Log.Information("Running version detected as {Version}", thisVersion);
            Log.Information("Telemetry Enabled: {TelemetryIsEnabled}", Telemetry.Current.IsEnabled().ToString());
            Log.Information("Telemetry Note: We use Application Insights to collect telemetry on performance & feature usage for the tools to help our developers target features. This data is tied to a session ID that is generated and shown in the logs. This can help with debugging.");
            Log.Information("SessionID: {SessionID}", Telemetry.Current.Context.Session.Id);
            LogContext.PushProperty("SessionID", Telemetry.Current.Context.Session.Id);
            Log.Information("User: {UserId}", Telemetry.Current.Context.User.Id);
            Log.Information("Start Time: {StartTime}", startTime.ToUniversalTime().ToLocalTime());
            Log.Information("Running with {@Args}", args);
            ///////////////////////////////////////////////////////
            var aiServiceOptions = new ApplicationInsightsServiceOptions();
            aiServiceOptions.InstrumentationKey = Telemetry.Current.InstrumentationKey;
            aiServiceOptions.ApplicationVersion = thisVersion.ToString();
            ///////////////////////////////////////////////////////
            /// Setup Host
            var host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton<IDetectOnlineService, DetectOnlineService>();
                    services.AddSingleton<IDetectVersionService, DetectVersionService>();
                    services.AddApplicationInsightsTelemetryWorkerService(aiServiceOptions);
                })
                .UseSerilog()
                .Build();
            TelemetryClient telemetryClient = SetupTelemetry(host);
            var chk = CheckVersion(thisVersion, host);
            if (chk != 0)
            {
                return chk;
            }
            //////////////////////////////////////////////////
            //////////////////////////////////////////////////
            int result = (int)Parser.Default.ParseArguments<InitOptions, ExecuteOptions, ExportADGroupsOptions>(args).MapResult(
                (InitOptions opts) => RunInitAndReturnExitCode(opts),
                (ExecuteOptions opts) => RunExecuteAndReturnExitCode(opts),
                (ExportADGroupsOptions opts) => ExportADGroupsCommand.Run(opts, logsPath),
                errs => 1);
            //////////////////////////////////////////////////
            Log.Information("-Application END");
            mainTimer.Stop();
            Telemetry.Current.TrackEvent("ApplicationEnd", null,
                new Dictionary<string, double> {
                        { "ApplicationDuration", mainTimer.ElapsedMilliseconds }
                });
            if (Telemetry.Current != null)
            {
                Telemetry.Current.Flush();
                // Allow time for flushing:
                System.Threading.Thread.Sleep(1000);
            }
            Log.Information("The application ran in {Elapsed} and finished at {EndTime}", mainTimer.Elapsed.ToString("c"), DateTime.Now.ToUniversalTime().ToLocalTime());
#if DEBUG
            Log.Information("App paused so you can check the output.  Press a key to close.");
            Console.ReadKey();
#endif
            return result;
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            //ExceptionTelemetry excTelemetry = new ExceptionTelemetry((Exception)e.ExceptionObject);
            //excTelemetry.SeverityLevel = SeverityLevel.Critical;
            //excTelemetry.HandledAt = ExceptionHandledAt.Unhandled;
            Log.Error((Exception)e.ExceptionObject, "An Unhandled exception occured.");
            Telemetry.Current.Flush();
            System.Threading.Thread.Sleep(1000);
        }

        private static object RunExecuteAndReturnExitCode(ExecuteOptions opts)
        {
            Telemetry.Current.TrackEvent("ExecuteCommand");
            EngineConfiguration ec;
            if (opts.ConfigFile == string.Empty)
            {
                opts.ConfigFile = "configuration.json";
            }

            if (!File.Exists(opts.ConfigFile))
            {
                Log.Information("The config file does not exist, nor does the default 'configuration.json'. Use 'init' to create a configuration file first", "[Error]");
                return 1;
            }
            else
            {
                Log.Information("Loading Config");
                string configurationjson;
                using (var sr = new StreamReader(opts.ConfigFile))
                    configurationjson = sr.ReadToEnd();

                ec = JsonConvert.DeserializeObject<EngineConfiguration>(configurationjson,
                    new FieldMapConfigJsonConverter(),
                    new ProcessorConfigJsonConverter());

#if !DEBUG
                string appVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(2);
                if (ec.Version != appVersion)
                {
                    Log.Information("The config version {Version} does not match the current app version {appVersion}. There may be compatability issues and we recommend that you generate a new default config and then tranfer the settings accross.", ec.Version, appVersion);
                    return 1;
                }
#endif
            }
            Log.Information("Config Loaded, creating engine");

            VssCredentials sourceCredentials = null;
            VssCredentials targetCredentials = null;
            if (!string.IsNullOrWhiteSpace(opts.SourceUserName) && !string.IsNullOrWhiteSpace(opts.SourcePassword))
                sourceCredentials = new VssCredentials(new Microsoft.VisualStudio.Services.Common.WindowsCredential(new NetworkCredential(opts.SourceUserName, opts.SourcePassword, opts.SourceDomain)));

            if (!string.IsNullOrWhiteSpace(opts.TargetUserName) && !string.IsNullOrWhiteSpace(opts.TargetPassword))
                targetCredentials = new VssCredentials(new Microsoft.VisualStudio.Services.Common.WindowsCredential(new NetworkCredential(opts.TargetUserName, opts.TargetPassword, opts.TargetDomain)));

            MigrationEngine me;
            if (sourceCredentials == null && targetCredentials == null)
                me = new MigrationEngine(ec);
            else
                me = new MigrationEngine(ec, sourceCredentials, targetCredentials);

            if (!string.IsNullOrWhiteSpace(opts.ChangeSetMappingFile))
            {
                using (System.IO.StreamReader file = new System.IO.StreamReader(opts.ChangeSetMappingFile))
                {
                    string line = string.Empty;
                    while ((line = file.ReadLine()) != null)
                    {
                        if (string.IsNullOrEmpty(line))
                        {
                            continue;
                        }

                        var split = line.Split('-');
                        if (split == null
                            || split.Length != 2
                            || !int.TryParse(split[0], out int changesetId))
                        {
                            continue;
                        }

                        me.ChangeSetMapping.Add(changesetId, split[1]);
                    }
                }
            }

            Console.Title = $"Azure DevOps Migration Tools: {System.IO.Path.GetFileName(opts.ConfigFile)} - {System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(3)} - {ec.Source.Project} - {ec.Target.Project}";
            Log.Information("Engine created, running...");
            me.Run();
            Log.Information("Run complete...");
            return 0;
        }

        private static object RunInitAndReturnExitCode(InitOptions opts)
        {
            Telemetry.Current.TrackEvent("InitCommand");

            string configFile = opts.ConfigFile;
            if (configFile.IsEmpty())
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

        private static TelemetryClient SetupTelemetry(IHost host)
        {
            var telemetryClient = host.Services.GetRequiredService<TelemetryClient>();
            telemetryClient.Context.User.Id = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            telemetryClient.Context.Session.Id = Guid.NewGuid().ToString();
            telemetryClient.Context.Device.OperatingSystem = Environment.OSVersion.ToString();
            telemetryClient.Context.Component.Version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            return telemetryClient;
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
        private static int CheckVersion(Version ApplicationVersion, IHost host)
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
            Log.Information("===                       Azure DevOps Migration Tools                       ==");
            Log.Information($"===                                 v{thisVersion}                                ==");
            Log.Information("===============================================================================");
        }
    }
}
