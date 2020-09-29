using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using Microsoft.Extensions.DependencyInjection;
using MigrationTools;
using MigrationTools.CommandLine;
using MigrationTools.Core.Clients;
using MigrationTools.Core.Configuration;
using MigrationTools.Core.Engine;
using MigrationTools.Core.Engine.Containers;
using Serilog;

namespace MigrationTools.Core
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
                    IMigrationClient s = _services.GetRequiredService<IMigrationClient>();
                    s.Configure(config.Source, credentials.source);
                    Source = s;
            }
            if (config.Target != null)
            {
                IMigrationClient t = _services.GetRequiredService<IMigrationClient>();
                t.Configure(config.Target, credentials.target);
                Target = t;
            }
        }

        public IMigrationClient Source { get; private set; }

        public IMigrationClient Target { get; private set; }


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

    }
}
