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
using MigrationTools;
using Microsoft.Extensions.Configuration;

namespace VstsSyncMigrator.ConsoleApp
{
    public class Program : ProgramManager
    {
        static DateTime startTime = DateTime.Now;
        static Stopwatch mainTimer = new Stopwatch();

        public static int Main(string[] args)
        {
            var telemetryClient = GetTelemiteryClient();
            Log.Logger = BuildLogger();
            /////////////////////////////////////////////////////////
            Trace.Listeners.Add(new TextWriterTraceListener(Console.Out)); // TODO: Remove once Trace replaced with log
            var oldlogPath = Path.Combine(CreateLogsPath(), "old-migration.log"); // TODO: Remove once Trace replaced with log
            Trace.Listeners.Add(new TextWriterTraceListener(oldlogPath, "myListener")); // TODO: Remove once Trace replaced with log
            ///////////////////////////////////////////////////////////////////////////
            ApplicationStartup(args);
            var doService = new DetectOnlineService(telemetryClient);
            if (doService.IsOnline())
            {
                var dvService = new DetectVersionService(telemetryClient);
                Version latestVersion = dvService.GetLatestVersion();
                Log.Information("Latest version detected as {Version_Latest}", latestVersion);
                var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                if (latestVersion > version)
                {
                    Log.Warning("You are currently running version {Version_Current} and a newer version ({Version_Latest}) is available. You should upgrade now using Chocolatey command 'choco upgrade vsts-sync-migrator' from the command line.", version, latestVersion);
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
                (ExportADGroupsOptions opts) => ExportADGroupsCommand.Run(opts, oldlogPath),
                errs => 1);
            ApplicationShutdown();
#if DEBUG
            Log.Information("App paused so you can check the output.  Press a key to close.");
            Console.ReadKey();
#endif
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
            Log.Information("Config Found, creating engine host");
            var config = new EngineConfigurationBuilder().BuildFromFile(opts.ConfigFile);
            Console.Title = $"Azure DevOps Migration Tools: {System.IO.Path.GetFileName(opts.ConfigFile)} - {System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(3)} - {config.Source.Project} - {config.Target.Project}";
            //
            NetworkCredential sourceCredentials = null;
            NetworkCredential targetCredentials = null;

            if (!string.IsNullOrWhiteSpace(opts.SourceUserName) && !string.IsNullOrWhiteSpace(opts.SourcePassword))
                sourceCredentials = new NetworkCredential(opts.SourceUserName, opts.SourcePassword, opts.SourceDomain);//new VssCredentials(new Microsoft.VisualStudio.Services.Common.WindowsCredential(new NetworkCredential(opts.SourceUserName, opts.SourcePassword, opts.SourceDomain)));

            if (!string.IsNullOrWhiteSpace(opts.TargetUserName) && !string.IsNullOrWhiteSpace(opts.TargetPassword))
                targetCredentials = new NetworkCredential(opts.TargetUserName, opts.TargetPassword, opts.TargetDomain);//new VssCredentials(new Microsoft.VisualStudio.Services.Common.WindowsCredential(new NetworkCredential(opts.TargetUserName, opts.TargetPassword, opts.TargetDomain)));

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
                    services.AddSingleton<IEngineConfigurationBuilder, EngineConfigurationBuilder>();
                    services.AddSingleton<EngineConfiguration>(config);
                    services.AddSingleton<Engine.MigrationEngine>();
                })
                .UseConsoleLifetime()
                .UseSerilog()
                .Build();

            var me = host.Services.GetRequiredService<Engine.MigrationEngine>();
            //
            me.AddNetworkCredentials(sourceCredentials, targetCredentials);
            if (!string.IsNullOrWhiteSpace(opts.ChangeSetMappingFile))
            {
                IChangeSetMappingProvider csmp = new ChangeSetMappingProvider(opts.ChangeSetMappingFile);
                csmp.ImportMappings(me.ChangeSetMapping);
            }
            
            Log.Information("Engine created, running...");
            me.Run();
            Log.Information("Run complete...");
            return 0;
        }

    }
}
