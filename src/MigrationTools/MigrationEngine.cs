using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using Microsoft.Extensions.DependencyInjection;
using MigrationTools;
using MigrationTools.CommandLine;
using MigrationTools.Clients;
using MigrationTools.Configuration;
using MigrationTools.Engine;
using MigrationTools.Engine.Containers;
using Serilog;

namespace MigrationTools
{
    public class MigrationEngine : IMigrationEngine
    {
        ExecuteOptions executeOptions;

        private readonly IServiceProvider _services;
        private readonly Guid _Guid = Guid.NewGuid();

        public ProcessorContainer Processors { get; }
        public TypeDefinitionMapContainer TypeDefinitionMaps { get; }
        public GitRepoMapContainer GitRepoMaps { get; }
        public ChangeSetMappingContainer ChangeSetMapps { get; }
        public FieldMapContainer FieldMaps { get; }
        public EngineConfiguration Config { get; }

        public ITelemetryLogger Telemetry { get; }

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
            Log.Information("Creating Migration Engine {Guid}", _Guid);
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

        public (NetworkCredential source, NetworkCredential target) CheckForNetworkCredentials()
        {
            NetworkCredential sourceCredentials = null;
            NetworkCredential targetCredentials = null;
            if (!string.IsNullOrWhiteSpace(executeOptions?.SourceUserName) && !string.IsNullOrWhiteSpace(executeOptions.SourcePassword))
                sourceCredentials = new NetworkCredential(executeOptions.SourceUserName, executeOptions.SourcePassword, executeOptions.SourceDomain);

            if (!string.IsNullOrWhiteSpace(executeOptions?.TargetUserName) && !string.IsNullOrWhiteSpace(executeOptions.TargetPassword))
                targetCredentials = new NetworkCredential(executeOptions.TargetUserName, executeOptions.TargetPassword, executeOptions.TargetDomain);

            return (sourceCredentials, targetCredentials);
        }

        private IMigrationClient _Source;

        public IMigrationClient Source { get {
                return GetSource();
            } }


        private IMigrationClient GetSource()
        {
            if (_Source is null)
            {
                var credentials = CheckForNetworkCredentials();
                if (Config.Source != null)
                {
                    _Source = _services.GetRequiredService<IMigrationClient>();
                    _Source.Configure(Config.Source, credentials.source);
                }
            }
            return _Source;
        }

        private IMigrationClient _Target;

        public IMigrationClient Target
        {
            get
            {
                return GetTarget();
            }
        }


        private IMigrationClient GetTarget()
        {
            if (_Target is null)
            {
                var credentials = CheckForNetworkCredentials();
                if (Config.Target != null)
                {
                    _Target = _services.GetRequiredService<IMigrationClient>();
                    _Target.Configure(Config.Target, credentials.target);
                }
            }
            return _Target;
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

			ProcessingStatus ps = ProcessingStatus.Running;

            Processors.EnsureConfigured();
            TypeDefinitionMaps.EnsureConfigured();
            GitRepoMaps.EnsureConfigured();
            ChangeSetMapps.EnsureConfigured();
            FieldMaps.EnsureConfigured();

            Log.Information("Beginning run of {ProcessorCount} processors", Processors.Count.ToString());
            foreach (IProcessor process in Processors.Items)
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

    }
}
