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
using MigrationTools.Sinks.TfsObjectModel;
using MigrationTools.Sinks.TfsObjectModel.FieldMaps;
using MigrationTools.Core.Engine.Containers;

namespace VstsSyncMigrator.ConsoleApp
{
    public class Program : ProgramManager
    {
        static DateTime startTime = DateTime.Now;
        static Stopwatch mainTimer = new Stopwatch();

        public static int Main(string[] args)
        {
            var telemetryClient = BuildTelemetryLogger();
            Log.Logger = BuildLogger(telemetryClient);

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
                (ExecuteOptions opts) => RunExecuteAndReturnExitCode(opts, telemetryClient, AddPlatformSpecificServices, ExecuteEntryPoint),
                (ExportADGroupsOptions opts) => ExportADGroupsCommand.Run(opts, oldlogPath, telemetryClient),
                errs => 1);
            ApplicationShutdown();
#if DEBUG
            Log.Information("App paused so you can check the output.  Press a key to close.");
            Console.ReadKey();
#endif
            return result;
        }

        public static void ExecuteEntryPoint(IHost host, ExecuteOptions opts)
        {
            var me = host.Services.GetRequiredService<Engine.MigrationEngine>();

            NetworkCredential sourceCredentials = null;
            NetworkCredential targetCredentials = null;

            if (!string.IsNullOrWhiteSpace(opts.SourceUserName) && !string.IsNullOrWhiteSpace(opts.SourcePassword))
                sourceCredentials = new NetworkCredential(opts.SourceUserName, opts.SourcePassword, opts.SourceDomain);//new VssCredentials(new Microsoft.VisualStudio.Services.Common.WindowsCredential(new NetworkCredential(opts.SourceUserName, opts.SourcePassword, opts.SourceDomain)));

            if (!string.IsNullOrWhiteSpace(opts.TargetUserName) && !string.IsNullOrWhiteSpace(opts.TargetPassword))
                targetCredentials = new NetworkCredential(opts.TargetUserName, opts.TargetPassword, opts.TargetDomain);//new VssCredentials(new Microsoft.VisualStudio.Services.Common.WindowsCredential(new NetworkCredential(opts.TargetUserName, opts.TargetPassword, opts.TargetDomain)));

            me.AddNetworkCredentials(sourceCredentials, targetCredentials);

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
            services.AddTransient<FieldToFieldMap>();
            services.AddTransient<FieldValueMap>();
            services.AddTransient<MultiValueConditionalMap>();
            services.AddTransient<RegexFieldMap>();
            services.AddTransient<TreeToTagFieldMap>();

            //Containers
            services.AddSingleton<FieldMapContainer>();

            //Engine
            services.AddSingleton<Engine.MigrationEngine>();

            //Processors
            services.AddSingleton<WorkItemMigrationContext>();
            services.AddSingleton<NodeStructuresMigrationContext>();
            services.AddSingleton<TeamMigrationContext>();
            services.AddSingleton<TestConfigurationsMigrationContext>();
            services.AddSingleton<TestPlandsAndSuitesMigrationContext>();
            services.AddSingleton<TestVeriablesMigrationContext>();
            services.AddSingleton<WorkItemPostProcessingContext>();
            services.AddSingleton<WorkItemQueryMigrationContext>();
            services.AddSingleton<CreateTeamFolders>();
            services.AddSingleton<ExportProfilePictureFromADContext>();
            services.AddSingleton<ExportTeamList>();
            services.AddSingleton<FixGitCommitLinks>();
            services.AddSingleton<ImportProfilePictureContext>();
            services.AddSingleton<WorkItemDelete>();
            services.AddSingleton<WorkItemUpdate>();
            services.AddSingleton<WorkItemUpdateAreasAsTagsContext>();



            return services;
        }


    }
}
