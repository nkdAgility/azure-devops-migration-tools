using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.VisualStudio.Services.Common;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using VstsSyncMigrator.Engine.ComponentContext;
using MigrationTools.Core.Configuration;
using MigrationTools.Core.Configuration.FieldMap;
using MigrationTools.Core.Configuration.Processing;
using MigrationTools.Core.Engine;
using Microsoft.Extensions.Hosting;
using System.Net;
using MigrationTools;
using Microsoft.Extensions.DependencyInjection;
using MigrationTools.Core.Engine.Containers;
using Microsoft.ApplicationInsights;

namespace VstsSyncMigrator.Engine
{
   public class MigrationEngine
    {

        NetworkCredential sourceCreds;
        NetworkCredential targetCreds;

        private readonly IServiceProvider _services;
        private readonly Guid _Guid = Guid.NewGuid();

        public ProcessorContainer Processors { get; }
        public TypeDefinitionMapContainer TypeDefinitionMaps { get; }
        public GitRepoMapContainer GitRepoMaps { get; }
        public ChangeSetMappingContainer ChangeSetMapps { get; }
        public FieldMapContainer FieldMaps { get; }

        public ITelemetryLogger Telemetry { get; }

        public MigrationEngine(EngineConfiguration config, 
            TypeDefinitionMapContainer typeDefinitionMaps, 
            ProcessorContainer processors, 
            GitRepoMapContainer gitRepoMaps,
            ChangeSetMappingContainer changeSetMapps,
            FieldMapContainer fieldMaps,
            ITelemetryLogger telemetry)
        {
            Log.Information("Creating Migration Engine {Guid}", _Guid);
            FieldMaps = fieldMaps;
            TypeDefinitionMaps = typeDefinitionMaps;
            Processors = processors;
            GitRepoMaps = gitRepoMaps;
            ChangeSetMapps = changeSetMapps;
            Telemetry = telemetry;
            ProcessConfiguration(config);
        }

        public void AddNetworkCredentials(NetworkCredential sourceCredentials, NetworkCredential targetCredentials)
        {
            sourceCreds = sourceCredentials;
            targetCreds = targetCredentials;
        }

        private void ProcessConfiguration(EngineConfiguration config)
        {
            if (config.Source != null)
            {
                if (sourceCreds == null)
                    SetSource(new TeamProjectContext(config.Source, Telemetry));
                else
                    SetSource(new TeamProjectContext(config.Source, sourceCreds, Telemetry));
            }
            if (config.Target != null)
            {
                if (targetCreds == null)
                    SetTarget(new TeamProjectContext(config.Target, Telemetry));
                else
                    SetTarget(new TeamProjectContext(config.Target, targetCreds, Telemetry));
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
            foreach (ITfsProcessingContext process in Processors.Items)
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
