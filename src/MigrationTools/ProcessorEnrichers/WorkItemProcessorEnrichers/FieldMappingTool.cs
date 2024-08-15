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

namespace MigrationTools.ProcessorEnrichers.WorkItemProcessorEnrichers
{
    public class FieldMappingTool : WorkItemProcessorEnricher
    {
        private ILogger<WorkItemProcessorEnricher> _logger;
        private FieldMappingToolOptions _Options;

        private Dictionary<string, List<IFieldMap>> fieldMapps = new Dictionary<string, List<IFieldMap>>();

        public int Count { get { return fieldMapps.Count; } }

        public Dictionary<string, List<IFieldMap>> Items
        {
            get { return fieldMapps; }
        }

        public FieldMappingTool(IOptions<FieldMappingToolOptions> options, IServiceProvider services, ILogger<WorkItemProcessorEnricher> logger, ITelemetryLogger telemetry) : base(services, logger, telemetry)
        {
            _logger = logger;
            _Options = options.Value;
            if (_Options.FieldMaps != null)
            {
                foreach (IFieldMapConfig fieldmapConfig in _Options.FieldMaps)
                {
                    _logger.LogInformation("FieldMappingTool: Adding FieldMap {FieldMapName} for {WorkItemTypeName}", fieldmapConfig.FieldMap, fieldmapConfig.WorkItemTypeName);
                    string typePattern = $"MigrationTools.Sinks.*.FieldMaps.{fieldmapConfig.FieldMap}";

                    Type type = AppDomain.CurrentDomain.GetAssemblies()
                             .Where(a => !a.IsDynamic)
                             .SelectMany(a => a.GetTypes())
                             .FirstOrDefault(t => t.Name.Equals(fieldmapConfig.FieldMap) || t.FullName.Equals(typePattern));

                    if (type == null)
                    {
                        _logger.LogError("Type " + typePattern + " not found.", typePattern);
                        throw new Exception("Type " + typePattern + " not found.");
                    }
                    IFieldMap fm = (IFieldMap)Services.GetRequiredService(type);
                    fm.Configure(fieldmapConfig);
                    AddFieldMap(fieldmapConfig.WorkItemTypeName, fm);
                }
            }

        }


        [Obsolete]
        public override void Configure(IProcessorEnricherOptions options)
        {
            throw new NotImplementedException();
        }


        protected override void EntryForProcessorType(Processors.IProcessor processor)
        {
            throw new NotImplementedException();
        }



        protected override void RefreshForProcessorType(Processors.IProcessor processor)
        {
            throw new NotImplementedException();
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
                _logger.LogDebug("Running Field Map: {MapName} {MappingDisplayName}", map.Name, map.MappingDisplayName);
                map.Execute(source, target);
            }
        }
    }
}
