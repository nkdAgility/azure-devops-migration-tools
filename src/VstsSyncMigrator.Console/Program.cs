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

namespace VstsSyncMigrator.ConsoleApp
{
    public class Program
    {
        [Verb("init", HelpText = "Creates initial config file")]
        class InitOptions
        {
            //normal options here
        }
        [Verb("execute", HelpText = "Record changes to the repository.")]
        class RunOptions
        {
            [Option('c', "config", Required = true, HelpText = "Configuration file to be processed.")]
            public string ConfigFile { get; set; }
        }

        public static int Main(string[] args)
        {
            Telemetry.Current.TrackEvent("ApplicationStart");
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            DateTime startTime = DateTime.Now;
            Stopwatch mainTimer = new Stopwatch();
            mainTimer.Start();
            //////////////////////////////////////////////////
            Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));
            Trace.Listeners.Add(new TextWriterTraceListener(string.Format(@"{0}-{1}.log", DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss"), "Run"), "myListener"));
            //////////////////////////////////////////////////
            Trace.WriteLine(System.Reflection.Assembly.GetExecutingAssembly().GetName().Name, "[Info]");
            Version thisVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            Trace.WriteLine(string.Format("Running version detected as {0}", thisVersion), "[Info]");
            if (IsOnline())
            {
                Version latestVersion = GetLatestVersion();
                Trace.WriteLine(string.Format("Latest version detected as {0}", latestVersion), "[Info]");
                if (latestVersion > thisVersion)
                {
                    Trace.WriteLine(
                        string.Format("You are currenlty running version {0} and a newer version ({1}) is available. You should upgrade now using Chocolatey command 'choco update vsts-sync-migrator' from the command line.",
                        thisVersion, latestVersion
                        ),
                        "[Warning]");
#if DEBUG

                    Console.WriteLine("Do you want to continue? (y/n)");
                    if (Console.ReadKey().Key != ConsoleKey.Y)
                    {
                        Trace.WriteLine("User aborted to update version", "[Warning]");
                        return 2;
                    }
#endif
                }
            }
            Trace.WriteLine(string.Format("Telemitery Enabled: {0}", Telemetry.Current.IsEnabled().ToString()), "[Info]");
            Trace.WriteLine(string.Format("SessionID: {0}", Telemetry.Current.Context.Session.Id), "[Info]");
            Trace.WriteLine(string.Format("User: {0}", Telemetry.Current.Context.User.Id), "[Info]");
            Trace.WriteLine(string.Format("Start Time: {0}", startTime.ToUniversalTime()), "[Info]");
            Trace.WriteLine("------------------------------START-----------------------------", "[Info]");
            //////////////////////////////////////////////////
            int result = (int)Parser.Default.ParseArguments<InitOptions, RunOptions>(args).MapResult(
                (InitOptions opts) => RunInitAndReturnExitCode(opts),
                (RunOptions opts) => RunExecuteAndReturnExitCode(opts),
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
            Trace.WriteLine(string.Format("End Time: {0}", startTime.ToUniversalTime()), "[Info]");
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
                opts.ConfigFile = "vstsbulkeditor.json";
            }

            if (!File.Exists(opts.ConfigFile))
            {
                Trace.WriteLine("The config file does not exist, nor doe the default 'vstsbulkeditor.json'. Use 'init' to create a configuration file first", "[Error]");
                return 1;
            }
            else
            {
                Trace.WriteLine("Loading Config");
                StreamReader sr = new StreamReader(opts.ConfigFile);
                string vstsbulkeditorjson = sr.ReadToEnd();
                sr.Close();
                ec = JsonConvert.DeserializeObject<EngineConfiguration>(vstsbulkeditorjson, 
                    new FieldMapConfigJsonConverter(),
                    new ProcessorConfigJsonConverter());
            }
            Trace.WriteLine("Config Loaded, creating engine", "[Info]");
            MigrationEngine me = new MigrationEngine(ec);
            Trace.WriteLine("Engine created, running...", "[Info]");
            me.Run();
            Trace.WriteLine("Run complete...", "[Info]");
            return 0;
        }

        private static object RunInitAndReturnExitCode(InitOptions opts)
        {
            Telemetry.Current.TrackEvent("InitCommand");
            if (!File.Exists("vstsbulkeditor.json"))
            {
                string json = JsonConvert.SerializeObject(EngineConfiguration.GetDefault(),
                    new FieldMapConfigJsonConverter(),
                    new ProcessorConfigJsonConverter());
                StreamWriter sw = new StreamWriter("vstsbulkeditor.json");
                sw.WriteLine(json);
                sw.Close();
                Trace.WriteLine("New vstsbulkeditor.json file has been created", "[Info]");
            }
            return 0;
        }

        private static Version GetLatestVersion()
        {
            string packageID = "vsts-sync-migrator";

            //Connect to the official package repository
            IPackageRepository repo = PackageRepositoryFactory.Default.CreateRepository("https://chocolatey.org/api/v2/");
            var version = repo.FindPackagesById(packageID).Max(p => p.Version);
            return new Version(version.ToString());
        }

        private static bool IsOnline()
        {
            Ping myPing = new Ping();
            String host = "8.8.4.4";
            byte[] buffer = new byte[32];
            int timeout = 1000;
            PingOptions pingOptions = new PingOptions();
            PingReply reply = myPing.Send(host, timeout, buffer, pingOptions);
            if (reply.Status == IPStatus.Success)
            {
                return true;
            }
            return false;
        }
    }
}