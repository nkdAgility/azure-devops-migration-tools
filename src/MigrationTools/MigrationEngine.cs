using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using Microsoft.Extensions.DependencyInjection;
using MigrationTools._EngineV1.Containers;
using MigrationTools.Clients;
using MigrationTools.CommandLine;
using MigrationTools.Configuration;
using MigrationTools.Processors;
using Serilog;
using Serilog.Core;

namespace MigrationTools
{
    public class MigrationEngine : IMigrationEngine
    {
        private readonly IServiceProvider _services;
        private IMigrationClient _Source;
        private IMigrationClient _Target;
        private ExecuteOptions executeOptions;

        public MigrationEngine(
            IServiceProvider services,
            ExecuteOptions executeOptions,
            EngineConfiguration config,
            TypeDefinitionMapContainer typeDefinitionMaps,
            ProcessorContainer processors,
            GitRepoMapContainer gitRepoMaps,
            ChangeSetMappingContainer changeSetMapps,
            FieldMapContainer fieldMaps,
            ITelemetryLogger telemetry)
        {
            Log.Information("Creating Migration Engine {SessionId}", telemetry.SessionId);
            _services = services;
            FieldMaps = fieldMaps;
            this.executeOptions = executeOptions;
            TypeDefinitionMaps = typeDefinitionMaps;
            Processors = processors;
            GitRepoMaps = gitRepoMaps;
            ChangeSetMapps = changeSetMapps;
            Telemetry = telemetry;
            Config = config;
        }

        public ChangeSetMappingContainer ChangeSetMapps { get; }
        public EngineConfiguration Config { get; }
        public FieldMapContainer FieldMaps { get; }
        public GitRepoMapContainer GitRepoMaps { get; }
        public ProcessorContainer Processors { get; }

        public IMigrationClient Source
        {
            get
            {
                return GetSource();
            }
        }

        public IMigrationClient Target
        {
            get
            {
                return GetTarget();
            }
        }

        public ITelemetryLogger Telemetry { get; }
        public TypeDefinitionMapContainer TypeDefinitionMaps { get; }

        public NetworkCredential CheckForNetworkCredentials_Source()
        {
            NetworkCredential sourceCredentials = null;
            if (!string.IsNullOrWhiteSpace(executeOptions?.SourceUserName) && !string.IsNullOrWhiteSpace(executeOptions.SourcePassword))
            {
                sourceCredentials = new NetworkCredential(executeOptions.SourceUserName, executeOptions.SourcePassword, executeOptions.SourceDomain);
            }
            return sourceCredentials;
        }

        public NetworkCredential CheckForNetworkCredentials_Target()
        {
            NetworkCredential targetCredentials = null;
            if (!string.IsNullOrWhiteSpace(executeOptions?.TargetUserName) && !string.IsNullOrWhiteSpace(executeOptions.TargetPassword))
                targetCredentials = new NetworkCredential(executeOptions.TargetUserName, executeOptions.TargetPassword, executeOptions.TargetDomain);
            return targetCredentials;
        }

        public ProcessingStatus Run()
        {
            Telemetry.TrackEvent("EngineStart",
                new Dictionary<string, string> {
                    { "Engine", "Migration" }
                },
                new Dictionary<string, double> {
                    { "Processors", Processors.Count },
                    { "Mappings", FieldMaps.Count }
                });
            Stopwatch engineTimer = Stopwatch.StartNew();

            LoggingLevelSwitch logLevel = _services.GetRequiredService<LoggingLevelSwitch>();
            logLevel.MinimumLevel = Config.LogLevel;
            Log.Information("Logging has been configured and is set to: {LogLevel}. ", Config.LogLevel.ToString());
            Log.Information("                              Max Logfile: {FileLogLevel}. ", "Verbose");
            Log.Information("                              Max Console: {ConsoleLogLevel}. ", "Debug");
            Log.Information("                 Max Application Insights: {AILogLevel}. ", "Error");
            Log.Information("The Max log levels above show where to go look for extra info. e.g. Even if you set the log level to Verbose you will only see that info in the Log File, however everything up to Debug will be in the Console.");

            ProcessingStatus ps = ProcessingStatus.Running;

            Processors.EnsureConfigured();
            TypeDefinitionMaps.EnsureConfigured();
            GitRepoMaps.EnsureConfigured();
            ChangeSetMapps.EnsureConfigured();
            FieldMaps.EnsureConfigured();

            Log.Information("Beginning run of {ProcessorCount} processors", Processors.Count.ToString());
            foreach (_EngineV1.Containers.IProcessor process in Processors.Items)
            {
                Log.Information("Processor: {ProcessorName}", process.Name);
                Stopwatch processorTimer = Stopwatch.StartNew();
                process.Execute();
                processorTimer.Stop();
                Telemetry.TrackEvent("ProcessorComplete", new Dictionary<string, string> { { "Processor", process.Name }, { "Status", process.Status.ToString() } }, new Dictionary<string, double> { { "ProcessingTime", processorTimer.ElapsedMilliseconds } });

                if (process.Status == ProcessingStatus.Failed)
                {
                    ps = ProcessingStatus.Failed;
                    Log.Error("{Context} The Processor {ProcessorName} entered the failed state...stopping run", process.Name, "MigrationEngine");
                    break;
                }
            }
            engineTimer.Stop();
            Telemetry.TrackEvent("EngineComplete",
                new Dictionary<string, string> {
                    { "Engine", "Migration" }
                },
                new Dictionary<string, double> {
                    { "EngineTime", engineTimer.ElapsedMilliseconds }
                });
            return ps;
        }

        private IMigrationClient GetSource()
        {
            if (_Source is null)
            {
                var credentials = CheckForNetworkCredentials_Source();
                if (_Source == null)
                {
                    _Source = _services.GetRequiredService<IMigrationClient>();
                    _Source.Configure(Config.Source, credentials);
                }
            }
            return _Source;
        }

        private IMigrationClient GetTarget()
        {
            if (_Target is null)
            {
                var credentials = CheckForNetworkCredentials_Target();
                if (_Target == null)
                {
                    _Target = _services.GetRequiredService<IMigrationClient>();
                    _Target.Configure(Config.Target, credentials);
                }
            }
            return _Target;
        }
    }
}