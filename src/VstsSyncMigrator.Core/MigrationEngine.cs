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

namespace VstsSyncMigrator.Engine
{
   public class MigrationEngine
    {
        Dictionary<string, List<ComponentContext.IFieldMap>> fieldMapps = new Dictionary<string, List<ComponentContext.IFieldMap>>();
        
        NetworkCredential sourceCreds;
        NetworkCredential targetCreds;
        
        private readonly IHost _Host;
        private readonly Guid _Guid = Guid.NewGuid();

        public ProcessorContainer Processors { get; }
        public TypeDefinitionMapContainer TypeDefinitionMaps { get; }
        public GitRepoMapContainer GitRepoMaps { get; }
        public ChangeSetMappingContainer ChangeSetMapps { get; }

        public MigrationEngine(IHost host, EngineConfiguration config)
        {
            Log.Information("Creating Migration Engine {Guid}", _Guid);
            _Host = host;
            TypeDefinitionMaps = _Host.Services.GetRequiredService<TypeDefinitionMapContainer>();
            Processors = _Host.Services.GetRequiredService<ProcessorContainer>();
            GitRepoMaps = _Host.Services.GetRequiredService<GitRepoMapContainer>();
            ChangeSetMapps = _Host.Services.GetRequiredService<ChangeSetMappingContainer>();
            ProcessConfiguration(config);
        }

        public void AddNetworkCredentials(NetworkCredential sourceCredentials, NetworkCredential targetCredentials)
        {
            sourceCreds = sourceCredentials;
            targetCreds = targetCredentials;
        }

        private void ProcessConfiguration(EngineConfiguration config)
        {
            Telemetry.EnableTrace = config.TelemetryEnableTrace;
            if (config.Source != null)
            {
                if (sourceCreds == null)
                    SetSource(new TeamProjectContext(config.Source));
                else
                    SetSource(new TeamProjectContext(config.Source, sourceCreds));
            }
            if (config.Target != null)
            {
                if (targetCreds == null)
                    SetTarget(new TeamProjectContext(config.Target));
                else
                    SetTarget(new TeamProjectContext(config.Target, targetCreds));
            }           
            if (config.FieldMaps != null)
            {
                foreach (IFieldMapConfig fieldmapConfig in config.FieldMaps)
                {
                    Log.Information("{Context}: Adding FieldMap {FieldMapName}", fieldmapConfig.FieldMap, "MigrationEngine");
                    string typePattern = $"VstsSyncMigrator.Engine.ComponentContext.{fieldmapConfig.FieldMap}";
                    Type t = Type.GetType(typePattern);
                    if (t == null)
                    {
                        Log.Error("Type " + typePattern + " not found.", typePattern);
                        throw new Exception("Type " + typePattern + " not found.");
                    }
                    this.AddFieldMap(fieldmapConfig.WorkItemTypeName, (ComponentContext.IFieldMap)Activator.CreateInstance(t, fieldmapConfig));
                }
            }
 
        }

        public ITeamProjectContext Source { get; private set; }

        public ITeamProjectContext Target { get; private set; }


        public ProcessingStatus Run()
        {
            Telemetry.Current.TrackEvent("EngineStart",
                new Dictionary<string, string> {
                    { "Engine", "Migration" }
                },
                new Dictionary<string, double> {
                    { "Processors", Processors.Count },
                    { "Mappings", fieldMapps.Count }
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
                Telemetry.Current.TrackEvent("ProcessorComplete", new Dictionary<string, string> { { "Processor", process.Name }, { "Status", process.Status.ToString() } }, new Dictionary<string, double> { { "ProcessingTime", processorTimer.ElapsedMilliseconds } });

                if (process.Status == ProcessingStatus.Failed)
                {
                    ps = ProcessingStatus.Failed;
                    Log.Error("{Context} The Processor {ProcessorName} entered the failed state...stopping run", process.Name, "MigrationEngine");
                    break;
                }
            }
            engineTimer.Stop();
            Telemetry.Current.TrackEvent("EngineComplete", 
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

        public void AddFieldMap(string workItemTypeName, ComponentContext.IFieldMap fieldToTagFieldMap)
        {
            if (!fieldMapps.ContainsKey(workItemTypeName))
            {
                fieldMapps.Add(workItemTypeName, new List<ComponentContext.IFieldMap>());
            }
            fieldMapps[workItemTypeName].Add(fieldToTagFieldMap);
        }


        internal void ApplyFieldMappings(WorkItem source, WorkItem target)
        { 
            if (fieldMapps.ContainsKey("*"))
            {
                ProcessFieldMapList(source, target, fieldMapps["*"]);
            }
            if (fieldMapps.ContainsKey(source.Type.Name))
            {
                ProcessFieldMapList(source, target, fieldMapps[source.Type.Name]);
            }
        }

        internal void ApplyFieldMappings(WorkItem target)
        {
            if (fieldMapps.ContainsKey("*"))
            {
                ProcessFieldMapList(target, target, fieldMapps["*"]);
            }
            if (fieldMapps.ContainsKey(target.Type.Name))
            {
                ProcessFieldMapList(target, target, fieldMapps[target.Type.Name]);
            }
        }

        private  void ProcessFieldMapList(WorkItem source, WorkItem target, List<ComponentContext.IFieldMap> list)
        {
            foreach (ComponentContext.IFieldMap map in list)
            {
                Log.Debug("{Context} Running Field Map: {MapName} {MappingDisplayName}", map.Name, map.MappingDisplayName);
                map.Execute(source, target);
            }
        }

    }
}
