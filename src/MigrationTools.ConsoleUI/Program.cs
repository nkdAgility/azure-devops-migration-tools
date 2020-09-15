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
using Microsoft.TeamFoundation.Build.WebApi;

namespace MigrationTools.ConsoleUI
{
    class Program : ProgramManager
    {
        

        static int Main(string[] args)
        {
            var telemetryClient = GetTelemiteryClient();
            Log.Logger = BuildLogger();
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
                errs => 1);
            ApplicationShutdown();
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
           var config = new EngineConfigurationBuilder().BuildFromFile();
           Console.Title = $"Azure DevOps Migration Tools: {System.IO.Path.GetFileName(opts.ConfigFile)} - {System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(3)} - {config.Source.Project} - {config.Target.Project}";
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
                    services.AddSingleton<MigrationEngine>();
                    services.AzureDevOpsWorkerServices(config);                    
                })
                .UseConsoleLifetime()
                .UseSerilog()
                .Build();
            
            var me = host.Services.GetRequiredService<MigrationEngine>();
            Log.Information("Engine created, running...");
            me.Run();
            Log.Information("Run complete...");
            return 0;
        }

    }
}
