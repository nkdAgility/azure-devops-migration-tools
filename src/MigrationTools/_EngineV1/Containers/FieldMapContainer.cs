using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using MigrationTools._EngineV1.Configuration;
using MigrationTools._EngineV1.DataContracts;
using Serilog;

namespace MigrationTools._EngineV1.Containers
{
    public class FieldMapContainer : EngineContainer<Dictionary<string, List<IFieldMap>>>
    {
        private Dictionary<string, List<IFieldMap>> fieldMapps = new Dictionary<string, List<IFieldMap>>();

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
                    Log.Information("FieldMapContainer: Adding FieldMap {FieldMapName} for {WorkItemTypeName}", fieldmapConfig.FieldMap, fieldmapConfig.WorkItemTypeName);
                    string typePattern = $"MigrationTools.Sinks.*.FieldMaps.{fieldmapConfig.FieldMap}";

                    Type type = AppDomain.CurrentDomain.GetAssemblies()
                             .Where(a => !a.IsDynamic)
                             .SelectMany(a => a.GetTypes())
                             .FirstOrDefault(t => t.Name.Equals(fieldmapConfig.FieldMap) || t.FullName.Equals(typePattern));

                    if (type == null)
                    {
                        Log.Error("Type " + typePattern + " not found.", typePattern);
                        throw new Exception("Type " + typePattern + " not found.");
                    }
                    IFieldMap fm = (IFieldMap)Services.GetRequiredService(type);
                    fm.Configure(fieldmapConfig);
                    AddFieldMap(fieldmapConfig.WorkItemTypeName, fm);
                }
            }
        }

        public void AddFieldMap(string workItemTypeName, IFieldMap fieldToTagFieldMap)
        {
            if (string.IsNullOrEmpty(workItemTypeName))
            {
                throw new IndexOutOfRangeException("workItemTypeName on all fieldmaps must be set to at least '*'.");
            }
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