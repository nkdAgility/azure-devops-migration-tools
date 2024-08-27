using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MigrationTools._EngineV1.Configuration;
using MigrationTools._EngineV1.Containers;
using MigrationTools.DataContracts;
using MigrationTools.Enrichers;
using MigrationTools.Processors;
using MigrationTools.Tools.Infrastructure;
using MigrationTools.Tools.Interfaces;

namespace MigrationTools.Tools
{
    public class FieldMappingTool : Tool<FieldMappingToolOptions>, IFieldMappingTool
    {

        private Dictionary<string, List<IFieldMap>> fieldMapps = new Dictionary<string, List<IFieldMap>>();

        public FieldMappingTool(IOptions<FieldMappingToolOptions> options, IServiceProvider services, ILogger<ITool> logger, ITelemetryLogger telemetry) : base(options, services, logger, telemetry)
        {
            if (Options.FieldMaps != null)
            {
                foreach (IFieldMapOptions fieldmapConfig in Options.FieldMaps)
                {
                    Log.LogInformation("FieldMappingTool: Adding FieldMap {FieldMapName} for {WorkItemTypeName}", fieldmapConfig.ConfigurationOptionFor, fieldmapConfig.ApplyTo.Count == 0?  "*ApplyTo is missing*" : string.Join(", ", fieldmapConfig.ApplyTo));
                    string typePattern = $"MigrationTools.Sinks.*.FieldMaps.{fieldmapConfig.ConfigurationOptionFor}";

                    Type type = AppDomain.CurrentDomain.GetAssemblies()
                             .Where(a => !a.IsDynamic)
                             .SelectMany(a => a.GetTypes())
                             .FirstOrDefault(t => t.Name.Equals(fieldmapConfig.ConfigurationOptionFor, StringComparison.InvariantCultureIgnoreCase) || t.FullName.Equals(typePattern));

                    if (type == null)
                    {
                        Log.LogError("Type " + typePattern + " not found.", typePattern);
                        throw new Exception("Type " + typePattern + " not found.");
                    }
                    IFieldMap fm = (IFieldMap)Services.GetRequiredService(type);
                    fm.Configure(fieldmapConfig);
                    foreach (string workItemTypeName in fieldmapConfig.ApplyTo)
                    {
                        AddFieldMap(workItemTypeName, fm);
                    }
                }
            }
        }

        public int Count { get { return fieldMapps.Count; } }

        public Dictionary<string, List<IFieldMap>> Items
        {
            get { return fieldMapps; }
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
            if (fieldMapps.ContainsKey(source.Fields["System.WorkItemType"].Value.ToString()))
            {
                ProcessFieldMapList(source, target, fieldMapps[source.Fields["System.WorkItemType"].Value.ToString()]);
            }
        }

        public void ApplyFieldMappings(WorkItemData target)
        {
            if (fieldMapps.ContainsKey("*"))
            {
                ProcessFieldMapList(target, target, fieldMapps["*"]);
            }
            if (fieldMapps.ContainsKey(target.Fields["System.WorkItemType"].Value.ToString()))
            {
                ProcessFieldMapList(target, target, fieldMapps[target.Fields["System.WorkItemType"].Value.ToString()]);
            }
        }

        private void ProcessFieldMapList(WorkItemData source, WorkItemData target, List<IFieldMap> list)
        {
            foreach (IFieldMap map in list)
            {
                Log.LogDebug("Running Field Map: {MapName} {MappingDisplayName}", map.Name, map.MappingDisplayName);
                map.Execute(source, target);
            }
        }
    }
}
