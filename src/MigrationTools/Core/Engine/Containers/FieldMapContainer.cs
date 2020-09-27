using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MigrationTools.Core.Configuration;
using MigrationTools.Core.DataContracts;
using Serilog;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace MigrationTools.Core.Engine.Containers
{
   public class FieldMapContainer : EngineContainer<Dictionary<string, List<IFieldMap>>>
    {
       public  Dictionary<string, List<IFieldMap>> fieldMapps = new Dictionary<string, List<IFieldMap>>();

        public FieldMapContainer(IServiceProvider services, EngineConfiguration config) : base(services, config)
        {
        }

         public int Count { get { return fieldMapps.Count; } }

        public override Dictionary<string, List<IFieldMap>> Items
        {
            get { return fieldMapps; }
        }

        protected override void Configure()
        {
            if (Config.FieldMaps != null)
            {
                foreach (IFieldMapConfig fieldmapConfig in Config.FieldMaps)
                {
                    Log.Information("{Context}: Adding FieldMap {FieldMapName}", fieldmapConfig.FieldMap, "MigrationEngine");
                    string typePattern = $"VstsSyncMigrator.Engine.ComponentContext.{fieldmapConfig.FieldMap}";
                    Type t = Type.GetType(typePattern);
                    if (t == null)
                    {
                        Log.Error("Type " + typePattern + " not found.", typePattern);
                        throw new Exception("Type " + typePattern + " not found.");
                    }
                    IFieldMap fm = (IFieldMap)Services.GetRequiredService(t);
                    fm.Configure(fieldmapConfig);
                    this.AddFieldMap(fieldmapConfig.WorkItemTypeName, fm);
                }
            }
        }

        public void AddFieldMap(string workItemTypeName, IFieldMap fieldToTagFieldMap)
        {
            if (!fieldMapps.ContainsKey(workItemTypeName))
            {
                fieldMapps.Add(workItemTypeName, new List<IFieldMap>());
            }
            fieldMapps[workItemTypeName].Add(fieldToTagFieldMap);
        }

        public void ApplyFieldMappings(WorkItemData source, WorkItemData target)
        {
            if (fieldMapps.ContainsKey("*"))
            {
                ProcessFieldMapList(source, target, fieldMapps["*"]);
            }
            if (fieldMapps.ContainsKey(source.Type))
            {
                ProcessFieldMapList(source, target, fieldMapps[source.Type]);
            }
        }

        public void ApplyFieldMappings(WorkItemData target)
        {
            if (fieldMapps.ContainsKey("*"))
            {
                ProcessFieldMapList(target, target, fieldMapps["*"]);
            }
            if (fieldMapps.ContainsKey(target.Type))
            {
                ProcessFieldMapList(target, target, fieldMapps[target.Type]);
            }
        }

        private void ProcessFieldMapList(WorkItemData source, WorkItemData target, List<IFieldMap> list)
        {
            foreach (IFieldMap map in list)
            {
                Log.Debug("{Context} Running Field Map: {MapName} {MappingDisplayName}", map.Name, map.MappingDisplayName);
                map.Execute(source, target);
            }
        }

    }
}
