using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using MigrationTools;
using MigrationTools.CommandLine;
using MigrationTools.Core.Configuration;
using MigrationTools.Core.Engine;
using MigrationTools.Core.Engine.Containers;
using Serilog;

namespace VstsSyncMigrator.Engine
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

        public ITelemetryLogger Telemetry { get; }

        public MigrationEngine(
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
            FieldMaps = fieldMaps;
            this.executeOptions = executeOptions;
            TypeDefinitionMaps = typeDefinitionMaps;
            Processors = processors;
            GitRepoMaps = gitRepoMaps;
            ChangeSetMapps = changeSetMapps;
            Telemetry = telemetry;
            ProcessConfiguration(config);
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

        private void ProcessConfiguration(EngineConfiguration config)
        {
            var credentials = CheckForNetworkCredentials();
            if (config.Source != null)
            {
                if (credentials.source == null)
                    SetSource(new TeamProjectContext(config.Source, Telemetry));
                else
                    SetSource(new TeamProjectContext(config.Source, credentials.source, Telemetry));
            }
            if (config.Target != null)
            {
                if (credentials.target == null)
                    SetTarget(new TeamProjectContext(config.Target, Telemetry));
                else
                    SetTarget(new TeamProjectContext(config.Target, credentials.target, Telemetry));
            }
        }

        public ITeamProjectContext Source { get; private set; }

        public ITeamProjectContext Target { get; private set; }


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
			ProcessingStatus ps = ProcessingStatus.Complete;
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

        public void SetSource(ITeamProjectContext teamProjectContext)
        {
            Source = teamProjectContext;
        }

        public void SetTarget(ITeamProjectContext teamProjectContext)
        {
            Target = teamProjectContext;
        }

    }
}
