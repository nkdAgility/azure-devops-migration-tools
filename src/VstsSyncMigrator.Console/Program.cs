using CommandLine.Text;
using CommandLine;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VstsSyncMigrator.Engine;
using VstsSyncMigrator.Engine.ComponentContext;
using System.IO;
using VstsSyncMigrator.Engine.Configuration;
using VstsSyncMigrator.Engine.Configuration.FieldMap;
using VstsSyncMigrator.Engine.Configuration.Processing;
using Microsoft.ApplicationInsights.DataContracts;
using NuGet;
using System.Net.NetworkInformation;
using VstsSyncMigrator.Commands;
using Microsoft.VisualStudio.Services.Common;
using System.Net;

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
            mainTimer.Start();
            Telemetry.Current.TrackEvent("ApplicationStart");
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            //////////////////////////////////////////////////
            string logsPath = CreateLogsPath();
            //////////////////////////////////////////////////
            Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));
            var logPath = Path.Combine(logsPath, "migration.log");
            Trace.Listeners.Add(new TextWriterTraceListener(logPath, "myListener"));
            Console.WriteLine("Writing log to " + logPath);
            //////////////////////////////////////////////////
            Version thisVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
           
            Trace.WriteLine(string.Format("Running version detected as {0}", thisVersion), "[Info]");
            if (IsOnline())
            {
                Version latestVersion = GetLatestVersion();
                Trace.WriteLine(string.Format("Latest version detected as {0}", latestVersion), "[Info]");
                if (latestVersion > thisVersion)
                {
                    Trace.WriteLine(
                        string.Format("You are currently running version {0} and a newer version ({1}) is available. You should upgrade now using Chocolatey command 'choco upgrade vsts-sync-migrator' from the command line.",
                        thisVersion, latestVersion
                        ),
                        "[Warning]");
#if !DEBUG

                    Console.WriteLine("Do you want to continue? (y/n)");
                    if (Console.ReadKey().Key != ConsoleKey.Y)
                    {
                        Trace.WriteLine("User aborted to update version", "[Warning]");
                        return 2;
                    }
#endif
                }
            }
            
            Trace.WriteLine(string.Format("Telemetry Enabled: {0}", Telemetry.Current.IsEnabled().ToString()), "[Info]");
            Trace.WriteLine("Telemetry Note: We use Application Insights to collect telemetry on performance & feature usage for the tools to help our developers target features. This data is tied to a session ID that is generated and shown in the logs. This can help with debugging.");
            Trace.WriteLine(string.Format("SessionID: {0}", Telemetry.Current.Context.Session.Id), "[Info]");
            Trace.WriteLine(string.Format("User: {0}", Telemetry.Current.Context.User.Id), "[Info]");
            Trace.WriteLine(string.Format("Start Time: {0}", startTime.ToUniversalTime().ToLocalTime()), "[Info]");
            AsciiLogo(thisVersion);
            //////////////////////////////////////////////////
            int result = (int)Parser.Default.ParseArguments<InitOptions, RunOptions, ExportADGroupsOptions>(args).MapResult(
                (InitOptions opts) => RunInitAndReturnExitCode(opts),
                (RunOptions opts) => RunExecuteAndReturnExitCode(opts),
                (ExportADGroupsOptions opts) => ExportADGroupsCommand.Run(opts, logsPath),
                errs => 1);
            //////////////////////////////////////////////////
            Trace.WriteLine("-------------------------------END------------------------------", "[Info]");
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
            Trace.WriteLine(string.Format("Duration: {0}", mainTimer.Elapsed.ToString("c")), "[Info]");
            Trace.WriteLine(string.Format("End Time: {0}", DateTime.Now.ToUniversalTime().ToLocalTime()), "[Info]");
#if DEBUG
            Console.ReadKey();
#endif
            return result;
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            ExceptionTelemetry excTelemetry = new ExceptionTelemetry((Exception)e.ExceptionObject);
            excTelemetry.SeverityLevel = SeverityLevel.Critical;
            excTelemetry.HandledAt = ExceptionHandledAt.Unhandled;
            Telemetry.Current.TrackException(excTelemetry);
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
                Trace.WriteLine("The config file does not exist, nor does the default 'configuration.json'. Use 'init' to create a configuration file first", "[Error]");
                return 1;
            }
            else
            {
                Trace.WriteLine("Loading Config");
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
                    Trace.WriteLine($"The config version {ec.Version} does not match the current app version {appVersion}. There may be compatability issues and we recommend that you generate a new default config and then tranfer the settings accross.", "[Info]");
                    return 1;
                }
#endif
            }
            Trace.WriteLine("Config Loaded, creating engine", "[Info]");

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
            Trace.WriteLine("Engine created, running...", "[Info]");
            me.Run();
            Trace.WriteLine("Run complete...", "[Info]");
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
            Telemetry.Current.TrackEvent("InitCommand");
            Trace.WriteLine(String.Format("ConfigFile: {0}", configFile), "[Info]");
            if (File.Exists(configFile))
            {
                Trace.WriteLine("Deleting old configuration.json reference file", "[Info]");
                File.Delete(configFile);
            }
            if (!File.Exists(configFile))
            {
                Trace.WriteLine(string.Format("Populating config with {0}", opts.Options.ToString()), "[Info]");
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
                Trace.WriteLine("New configuration.json file has been created", "[Info]");
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
                Telemetry.Current.TrackException(ex);
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
