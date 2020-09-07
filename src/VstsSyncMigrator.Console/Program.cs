using AzureDevOpsMigrationTools.Core.Configuration;
using AzureDevOpsMigrationTools.Core.Configuration.FieldMap;
using CommandLine;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
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
        [Verb("init", HelpText = "Creates initial config file")]
        class InitOptions
        {
            [Option('c', "config", Required = false, HelpText = "Configuration file to be processed.")]
            public string ConfigFile { get; set; }
            [Option('o', "options", Required = false, Default = OptionsMode.WorkItemTracking, HelpText = "Configuration file to be processed.")]
            public OptionsMode Options { get; set; }
        }

        public enum OptionsMode
        {
            Full = 0,
            WorkItemTracking = 1

        }

        [Verb("execute", HelpText = "Record changes to the repository.")]
        class RunOptions
        {
            [Option('c', "config", Required = true, HelpText = "Configuration file to be processed.")]
            public string ConfigFile { get; set; }

            [Option("sourceDomain", Required = false, HelpText = "Domain used to connect to the source TFS instance.")]
            public string SourceDomain { get; set; }

            [Option("sourceUserName", Required = false, HelpText = "User Name used to connect to the source TFS instance.")]
            public string SourceUserName { get; set; }

            [Option("sourcePassword", Required = false, HelpText = "Password used to connect to source TFS instance.")]
            public string SourcePassword { get; set; }

            [Option("targetDomain", Required = false, HelpText = "Domain used to connect to the target TFS instance.")]
            public string TargetDomain { get; set; }

            [Option("targetUserName", Required = false, HelpText = "User Name used to connect to the target TFS instance.")]
            public string TargetUserName { get; set; }

            [Option("targetPassword", Required = false, HelpText = "Password used to connect to target TFS instance.")]
            public string TargetPassword { get; set; }

            [Option("changeSetMappingFile", Required = false, HelpText = "Mapping between changeset id and commit id. Used to fix work item changeset links.")]
            public string ChangeSetMappingFile { get; set; }
        }

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

            if (IsOnline())
            {
                Version latestVersion = GetLatestVersion();
                log.Information("Latest version detected as {latestVersion}", latestVersion);
                if (latestVersion > thisVersion)
                {
                    log.Warning("You are currently running version {thisVersion} and a newer version ({latestVersion}) is available. You should upgrade now using Chocolatey command 'choco upgrade vsts-sync-migrator' from the command line.", thisVersion, latestVersion);
#if !DEBUG

                    Console.WriteLine("Do you want to continue? (y/n)");
                    if (Console.ReadKey().Key != ConsoleKey.Y)
                    {
                        log.Warning("User aborted to update version");
                        return 2;
                    }
#endif
                }
            }
            Log.Information("Telemetry Enabled: {TelemetryIsEnabled}", Telemetry.Current.IsEnabled().ToString());
            Log.Information("Telemetry Note: We use Application Insights to collect telemetry on performance & feature usage for the tools to help our developers target features. This data is tied to a session ID that is generated and shown in the logs. This can help with debugging.");
            Log.Information("SessionID: {SessionID}", Telemetry.Current.Context.Session.Id);
            LogContext.PushProperty("SessionID", Telemetry.Current.Context.Session.Id);
            Log.Information("User: {UserId}", Telemetry.Current.Context.User.Id);
            Log.Information("Start Time: {StartTime}", startTime.ToUniversalTime().ToLocalTime());
            Log.Information("Running with {@Args}", args);
            //////////////////////////////////////////////////
            int result = (int)Parser.Default.ParseArguments<InitOptions, RunOptions, ExportADGroupsOptions>(args).MapResult(
                (InitOptions opts) => RunInitAndReturnExitCode(opts),
                (RunOptions opts) => RunExecuteAndReturnExitCode(opts),
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

        private static object RunExecuteAndReturnExitCode(RunOptions opts)
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
                EngineConfiguration config;
                switch (opts.Options)
                {
                    case OptionsMode.Full:
                        config = EngineConfiguration.GetDefault();
                        break;
                    case OptionsMode.WorkItemTracking:
                        config = EngineConfiguration.GetWorkItemMigration();
                        break;
                    default:
                        config = EngineConfiguration.GetDefault();
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

        private static Version GetLatestVersion()
        {
            DateTime startTime = DateTime.Now;
            Stopwatch mainTimer = Stopwatch.StartNew();
            //////////////////////////////////
            string packageID = "vsts-sync-migrator";
            SemanticVersion version = SemanticVersion.Parse("0.0.0.0");
            bool sucess = false;
            try
            {
                //Connect to the official package repository
                IPackageRepository repo = PackageRepositoryFactory.Default.CreateRepository("https://chocolatey.org/api/v2/");
                SemanticVersion latestPackageVersion = repo.FindPackagesById(packageID).Max(p => p.Version);
                if (latestPackageVersion != null)
                {
                    version = latestPackageVersion;
                    sucess = true;
                }
            }
            catch (Exception ex)
            {
                Telemetry.Current.TrackException(ex);
                sucess = false;
            }
            /////////////////
            mainTimer.Stop();
            Telemetry.Current.TrackDependency(new DependencyTelemetry("PackageRepository", "chocolatey.org", "vsts-sync-migrator", version.ToString(), startTime, mainTimer.Elapsed, null, sucess));
            return new Version(version.ToString());
        }

        private static bool IsOnline()
        {
            DateTime startTime = DateTime.Now;
            Stopwatch mainTimer = Stopwatch.StartNew();
            //////////////////////////////////
            bool isOnline = false;
            string responce = "none";
            try
            {
                Ping myPing = new Ping();
                String host = "8.8.4.4";
                byte[] buffer = new byte[32];
                int timeout = 1000;
                PingOptions pingOptions = new PingOptions();
                PingReply reply = myPing.Send(host, timeout, buffer, pingOptions);
                responce = reply.Status.ToString();
                if (reply.Status == IPStatus.Success)
                {
                    isOnline = true;
                }
            }
            catch (Exception ex)
            {
                // Likley no network is even available
                Log.Error(ex, "Error checking if we are online.");
                responce = "error";
                isOnline = false;
            }
            /////////////////
            mainTimer.Stop();
            Telemetry.Current.TrackDependency(new DependencyTelemetry("Ping", "GoogleDNS", "IsOnline", null, startTime, mainTimer.Elapsed, responce, true));
            return isOnline;
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

        private static void AsciiLogo(Version thisVersion)
        {
            Console.WriteLine("                                      &@&                                      ");
            Console.WriteLine("                                   @@(((((@                                    ");
            Console.WriteLine("                                  @(((((((((@                                  ");
            Console.WriteLine("                                @(((((((((((((&                                ");
            Console.WriteLine("                              ##((((((@ @((((((@@                              ");
            Console.WriteLine("                             @((((((@     @((((((&                             ");
            Console.WriteLine("                            @(((((#        @((((((@                            ");
            Console.WriteLine("                           &(((((&           &(((((@                           ");
            Console.WriteLine("                          @(((((&             &(((((@                          ");
            Console.WriteLine("                          &(((((@#&@((.((&@@@(#(((((@                          ");
            Console.WriteLine("                         #((((#..................#@((&                         ");
            Console.WriteLine("                       &@(((((&......................(@                        ");
            Console.WriteLine("                     @.(&((((&...&&        &@&..........&@                     ");
            Console.WriteLine("                   @...@(((((@                   @#.......((                   ");
            Console.WriteLine("                 &.....@(((((@                   @((@.......&                  ");
            Console.WriteLine("                @......@(((((                    #((((&.......&                ");
            Console.WriteLine("               #.....( &(((((         @@@        ((((((@@......@               ");
            Console.WriteLine("              &.....@  @(((&@@#(((((((((((((((((#@(((((&  ......@              ");
            Console.WriteLine("             @.....@  &@&((((((((((((((((((((((((@(((((@#  ......@             ");
            Console.WriteLine("            @.....&@(((((((((((((((&&@@@@@(((((@((((#(((#@(....&               ");
            Console.WriteLine("            @.....&((((((((&@@&                 @(((((@(((((((@...#            ");
            Console.WriteLine("            &....((((((@@(((((@                &@(((((@&((((((((#&&            ");
            Console.WriteLine("           @(....&((@    @(((((@               @(((((@    @(((((((##           ");
            Console.WriteLine("         @(#(....&        &(((((@             @(((((&       &@(((((((&         ");
            Console.WriteLine("       &@(((&.....        @((((((&           @(((((       &.(&((((((@          ");
            Console.WriteLine("      @(((((@.....&        (((((@        &@(((((&         @....@((((((@        ");
            Console.WriteLine("     @(((((#@.....(          &(((((@&     ##(((((&         @.....@@((((((@     ");
            Console.WriteLine("   (&(((((@  &.....@&         @((((((@   @((((((@         @......   @(((((@    ");
            Console.WriteLine("   &(((((@    @.....#&         @#((((((@((((((#          @......&    @(((((@   ");
            Console.WriteLine("  @(((((@      &......&          @(((((((@#((@         &@......       @(((((@  ");
            Console.WriteLine(" @(((((@        @......@&        @@@(((((((&@&        @......(         #(((((@ ");
            Console.WriteLine(" #((((&           &.......@  &@&(((((@#((((((((@@& &@.......@          ((((&   ");
            Console.WriteLine("&(((((@@           @(....&@#((((((((((@ @(((((((#@........@            &@(((((@");
            Console.WriteLine("&(((((((((((((((((((((((((((((((((&@@@@@@@@@&...........@(((((((((((((((((((((@");
            Console.WriteLine("@(((((((((((((((((((((((((((((&@(....................@#((((((((((((((((((((((#@");
            Console.WriteLine("      @((((((((((((((&@&  &&...................@   @@#((((((((((((((#@@        ");
            Console.WriteLine("                                                                               ");
            Console.WriteLine("===============================================================================");
            Console.WriteLine("===                       Azure DevOps Migration Tools                       ==");
            Console.WriteLine($"===                                 v{thisVersion}                                ==");
            Console.WriteLine("===============================================================================");
        }
    }
}
