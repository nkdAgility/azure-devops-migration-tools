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

namespace VstsSyncMigrator.Engine
{
   public class MigrationEngine
    {
        List<ITfsProcessingContext> processors = new List<ITfsProcessingContext>();
        Dictionary<string, List<ComponentContext.IFieldMap>> fieldMapps = new Dictionary<string, List<ComponentContext.IFieldMap>>();
        Dictionary<string, IWitdMapper> workItemTypeDefinitions = new Dictionary<string, IWitdMapper>();
        Dictionary<string, string> gitRepoMapping = new Dictionary<string, string>();
        ITeamProjectContext source;
        ITeamProjectContext target;
        NetworkCredential sourceCreds;
        NetworkCredential targetCreds;
        public readonly Dictionary<int, string> ChangeSetMapping = new Dictionary<int, string>();
        private readonly IHost _Host;

        public MigrationEngine(IHost host)
        {
            _Host = host;
        }
        public MigrationEngine(IHost host, EngineConfiguration config)
        {
            ProcessConfiguration(config);
            _Host = host;
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
            if (config.GitRepoMapping != null)
            {
                gitRepoMapping = config.GitRepoMapping;
            }
            if (config.WorkItemTypeDefinition != null)
            { 
                foreach (string key in config.WorkItemTypeDefinition.Keys)
                {
                    Log.Information("{Context}: Adding Work Item Type {WorkItemType}", key, "MigrationEngine");
                    this.AddWorkItemTypeDefinition(key, new WitMapper(config.WorkItemTypeDefinition[key]));
                }
            }
            var enabledProcessors = config.Processors.Where(x => x.Enabled).ToList();
            foreach (ITfsProcessingConfig processorConfig in enabledProcessors)
            {
                if (processorConfig.IsProcessorCompatible(enabledProcessors))
                {
                    Log.Information("{Context}: Adding Processor {ProcessorName}", processorConfig.Processor, "MigrationEngine");
                    string typePattern = $"VstsSyncMigrator.Engine.{processorConfig.Processor}";
                    Type t = Type.GetType(typePattern);
                    if (t == null)
                    {
                        Log.Error("Type " + typePattern + " not found.", typePattern);
                        throw new Exception("Type " + typePattern + " not found.");
                    }
                    this.AddProcessor(
                        (ITfsProcessingContext)
                        Activator.CreateInstance(t, this, processorConfig));
                }
                else
                {
                    var message = "{Context}: Cannot add Processor {ProcessorName}. Processor is not compatible with other enabled processors in configuration.";
                    Log.Error(message, processorConfig.Processor, "MigrationEngine");
                    throw new InvalidOperationException(string.Format(message, processorConfig.Processor, "MigrationEngine"));
                }
            }
        }
        public Dictionary<string, string> GitRepoMappings
        {
            get
            {
                return gitRepoMapping;
            }
        }

        public Dictionary<string, IWitdMapper> WorkItemTypeDefinitions
        {
            get
            {
                return workItemTypeDefinitions;
            }
        }

        public ITeamProjectContext Source
        {
            get
            {
                return source;
            }
        }

        public ITeamProjectContext Target
        {
            get
            {
                return target;
            }
        }


        public ProcessingStatus Run()
        {
            Telemetry.Current.TrackEvent("EngineStart",
                new Dictionary<string, string> {
                    { "Engine", "Migration" }
                },
                new Dictionary<string, double> {
                    { "Processors", processors.Count },
                    { "Mappings", fieldMapps.Count }
                });
            Stopwatch engineTimer = Stopwatch.StartNew();
			ProcessingStatus ps = ProcessingStatus.Complete;
            Log.Error("{Context} Beginning run of {ProcessorCount} processors", processors.Count.ToString(), "MigrationEngine");
            foreach (ITfsProcessingContext process in processors)
            {
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

        public void AddProcessor<TProcessor>()
        {
            ITfsProcessingContext pc = (ITfsProcessingContext)Activator.CreateInstance(typeof(TProcessor), new object[] { this });
            AddProcessor(pc);
        }

        public void AddProcessor(ITfsProcessingContext processor)
        {
            processors.Add(processor);
        }

        public void SetSource(ITeamProjectContext teamProjectContext)
        {
            source = teamProjectContext;
        }

        public void SetTarget(ITeamProjectContext teamProjectContext)
        {
            target = teamProjectContext;
        }

        public void AddFieldMap(string workItemTypeName, ComponentContext.IFieldMap fieldToTagFieldMap)
        {
            if (!fieldMapps.ContainsKey(workItemTypeName))
            {
                fieldMapps.Add(workItemTypeName, new List<ComponentContext.IFieldMap>());
            }
            fieldMapps[workItemTypeName].Add(fieldToTagFieldMap);
        }
         public void AddWorkItemTypeDefinition(string workItemTypeName, IWitdMapper workItemTypeDefinitionMap = null)
        {
            if (!workItemTypeDefinitions.ContainsKey(workItemTypeName))
            {
                workItemTypeDefinitions.Add(workItemTypeName, workItemTypeDefinitionMap);
            }
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
