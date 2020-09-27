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
using MigrationTools.Sinks.AzureDevOps;

using System.Net;
using MigrationTools.Core.Engine;
using MigrationTools.Sinks.AzureDevOps.FieldMaps;

namespace MigrationTools.ConsoleUI
{
    class Program : ProgramManager
    {
        
        static int Main(string[] args)
        {
            var telemetryClient = BuildTelemetryLogger();
            Log.Logger = BuildLogger(telemetryClient);
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
                (ExecuteOptions opts) => RunExecuteAndReturnExitCode(opts, telemetryClient, AddPlatformSpecificServices, StartEngine),
                errs => 1);
            ApplicationShutdown();
            return result;
        }

        public static void StartEngine(IHost host, ExecuteOptions opts)
        {
            var me = host.Services.GetRequiredService<MigrationEngineCore>();
            NetworkCredential sourceCredentials = null;
            NetworkCredential targetCredentials = null;

            if (!string.IsNullOrWhiteSpace(opts.SourceUserName) && !string.IsNullOrWhiteSpace(opts.SourcePassword))
                sourceCredentials = new NetworkCredential(opts.SourceUserName, opts.SourcePassword, opts.SourceDomain);//new VssCredentials(new Microsoft.VisualStudio.Services.Common.WindowsCredential(new NetworkCredential(opts.SourceUserName, opts.SourcePassword, opts.SourceDomain)));

            if (!string.IsNullOrWhiteSpace(opts.TargetUserName) && !string.IsNullOrWhiteSpace(opts.TargetPassword))
                targetCredentials = new NetworkCredential(opts.TargetUserName, opts.TargetPassword, opts.TargetDomain);//new VssCredentials(new Microsoft.VisualStudio.Services.Common.WindowsCredential(new NetworkCredential(opts.TargetUserName, opts.TargetPassword, opts.TargetDomain)));
            //me.AddNetworkCredentials(sourceCredentials, targetCredentials);

            Log.Information("Engine created, running...");
            
            me.Run();
        }

        public static IServiceCollection AddPlatformSpecificServices(IServiceCollection services)
        {
            // Field Mapps
            services.AddTransient<FieldBlankMap>();
            services.AddTransient<FieldLiteralMap>();
            services.AddTransient<FieldMergeMap>();
            services.AddTransient<FieldToFieldMap>();
            services.AddTransient<FieldtoFieldMultiMap>();
            services.AddTransient<FieldToTagFieldMap>();
            services.AddTransient<FieldValuetoTagMap>();
            services.AddTransient<MultiValueConditionalMap>();
            services.AddTransient<RegexFieldMap>();
            services.AddTransient<TreeToTagFieldMap>();


            services.AddSingleton<MigrationEngineCore>();
            services.AddTransient<ITeamProjectContext, TeamProjectContext>();
            
            return services;
        }


    }
}
