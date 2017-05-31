using Microsoft.ApplicationInsights;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VstsSyncMigrator.Engine.ComponentContext;
using VstsSyncMigrator.Engine.Configuration;
using VstsSyncMigrator.Engine.Configuration.FieldMap;
using VstsSyncMigrator.Engine.Configuration.Processing;

namespace VstsSyncMigrator.Engine
{
   public class MigrationEngine
    {
        List<ITfsProcessingContext> processors = new List<ITfsProcessingContext>();
        List<Action<WorkItem, WorkItem>> processorActions = new List<Action<WorkItem, WorkItem>>();
        Dictionary<string, List<IFieldMap>> fieldMapps = new Dictionary<string, List<IFieldMap>>();
        Dictionary<string, IWitdMapper> workItemTypeDefinitions = new Dictionary<string, IWitdMapper>();
        ITeamProjectContext source;
        ITeamProjectContext target;
        string reflectedWorkItemIdFieldName = "TfsMigrationTool.ReflectedWorkItemId";
        private WorkItemStoreContext _sourceStore;
        private WorkItemStoreContext _targetStore;

        public MigrationEngine()
        {

        }
        public MigrationEngine(EngineConfiguration config)
        {
            ProcessConfiguration(config);
        }

        private void ProcessConfiguration(EngineConfiguration config)
        {
            Telemetry.EnableTrace = config.TelemetryEnableTrace;
            if (config.Source != null)
            {
                this.SetSource(new TeamProjectContext(config.Source.Collection, config.Source.Name));
            }
            if (config.Target != null)
            {
                this.SetTarget(new TeamProjectContext(config.Target.Collection, config.Target.Name));
            }           
            this.SetReflectedWorkItemIdFieldName(config.ReflectedWorkItemIDFieldName);
            if (config.FieldMaps != null)
            {
                foreach (IFieldMapConfig fieldmapConfig in config.FieldMaps)
                {
                    Trace.WriteLine(string.Format("Adding FieldMap {0}", fieldmapConfig.FieldMap.Name), "MigrationEngine");
                    this.AddFieldMap(fieldmapConfig.WorkItemTypeName, (IFieldMap)Activator.CreateInstance(fieldmapConfig.FieldMap, fieldmapConfig));
                }
            }            
            foreach (string key in config.WorkItemTypeDefinition.Keys)
            {
                Trace.WriteLine(string.Format("Adding Work Item Type {0}", key), "MigrationEngine");
                this.AddWorkItemTypeDefinition(key, new DescreteWitdMapper(config.WorkItemTypeDefinition[key]));
            }
            foreach (ITfsProcessingConfig processorConfig in config.Processors)
            {
                if (processorConfig.Enabled)
                {
                    Trace.WriteLine(string.Format("Adding Processor {0}", processorConfig.Processor.Name), "MigrationEngine");
                    this.AddProcessor((ITfsProcessingContext)Activator.CreateInstance(processorConfig.Processor, this, processorConfig));
                }
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

        public void SetStores(WorkItemStoreContext sourceStore, WorkItemStoreContext targetStore) {
            _sourceStore = sourceStore;
            _targetStore = targetStore;
        }

        public ITeamProjectContext Target
        {
            get
            {
                return target;
            }
        }
        public string ReflectedWorkItemIdFieldName
        {
            get { return reflectedWorkItemIdFieldName;}
        }

        public ProcessingStatus Run()
        {
            Telemetry.Current.TrackEvent("EngineStart",
                new Dictionary<string, string> {
                    { "Engine", "Migration" }
                },
                new Dictionary<string, double> {
                    { "Processors", processors.Count },
                    { "Actions",  processorActions.Count},
                    { "Mappings", fieldMapps.Count }
                });
            Stopwatch engineTimer = new Stopwatch();
            engineTimer.Start();
            ProcessingStatus ps = ProcessingStatus.Complete;
            Trace.WriteLine(string.Format("Beginning run of {0} processors", processors.Count.ToString()), "MigrationEngine");
            foreach (ITfsProcessingContext process in processors)
            {
                Stopwatch processorTimer = new Stopwatch();
                processorTimer.Start();
                process.Execute();
                processorTimer.Stop();
                Telemetry.Current.TrackEvent("ProcessorComplete", new Dictionary<string, string> { { "Processor", process.Name }, { "Status", process.Status.ToString() } }, new Dictionary<string, double> { { "ProcessingTime", processorTimer.ElapsedMilliseconds } });
                if (process.Status == ProcessingStatus.Failed)
                {
                    ps = ProcessingStatus.Failed;
                    Trace.WriteLine(string.Format("The Processor {0} entered the failed state...stopping run", process.Name), "MigrationEngine");
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


        public void SetReflectedWorkItemIdFieldName(string fieldName)
        {
            reflectedWorkItemIdFieldName = fieldName;
        }

        public void SetSource(ITeamProjectContext teamProjectContext)
        {
            source = teamProjectContext;
        }

        public void SetTarget(ITeamProjectContext teamProjectContext)
        {
            target = teamProjectContext;
        }

        public void AddFieldMap(string workItemTypeName, IFieldMap fieldToTagFieldMap)
        {
            if (!fieldMapps.ContainsKey(workItemTypeName))
            {
                fieldMapps.Add(workItemTypeName, new List<IFieldMap>());
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

        private  void ProcessFieldMapList(WorkItem source, WorkItem target, List<IFieldMap> list)
        {
            foreach (IFieldMap map in list)
            {
                Trace.WriteLine(string.Format("Runnin Field Map: {0}", map.Name));
                map.Init(this, _sourceStore, _targetStore);
                map.Execute(source, target);
            }
        }

    }
}
