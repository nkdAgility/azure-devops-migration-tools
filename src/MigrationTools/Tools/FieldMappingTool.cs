using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MigrationTools.DataContracts;
using MigrationTools.Tools.Infrastructure;
using MigrationTools.Tools.Interfaces;

namespace MigrationTools.Tools
{
    /// <summary>
    /// Tool for applying field mapping transformations to work items during migration, supporting various field mapping strategies like direct mapping, regex transformations, and value lookups.
    /// </summary>
    public class FieldMappingTool : Tool<FieldMappingToolOptions>, IFieldMappingTool
    {

        private readonly Dictionary<string, List<IFieldMap>> _fieldMaps = new Dictionary<string, List<IFieldMap>>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Initializes a new instance of the FieldMappingTool class.
        /// </summary>
        /// <param name="options">Configuration options for field mapping</param>
        /// <param name="services">Service provider for dependency injection</param>
        /// <param name="logger">Logger for the tool operations</param>
        /// <param name="telemetry">Telemetry logger for tracking operations</param>
        public FieldMappingTool(IOptions<FieldMappingToolOptions> options, IServiceProvider services, ILogger<ITool> logger, ITelemetryLogger telemetry) : base(options, services, logger, telemetry)
        {
            if (Options.FieldMaps != null)
            {
                foreach (IFieldMapOptions fieldmapConfig in Options.FieldMaps)
                {
                    Log.LogInformation("FieldMappingTool: Adding FieldMap {FieldMapName} for {WorkItemTypeName}", fieldmapConfig.ConfigurationMetadata.OptionFor, fieldmapConfig.ApplyTo.Count == 0 ? "*ApplyTo is missing*" : string.Join(", ", fieldmapConfig.ApplyTo));
                    string typePattern = $"MigrationTools.Sinks.*.FieldMaps.{fieldmapConfig.ConfigurationMetadata.OptionFor}";

                    Type type = AppDomain.CurrentDomain.GetAssemblies()
                             .Where(a => !a.IsDynamic)
                             .SelectMany(a => a.GetTypes())
                             .FirstOrDefault(t => t.Name.Equals(fieldmapConfig.ConfigurationMetadata.OptionFor, StringComparison.InvariantCultureIgnoreCase) || t.FullName.Equals(typePattern));

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

        public int Count => _fieldMaps.Count;

        public Dictionary<string, List<IFieldMap>> Items => _fieldMaps;

        public void AddFieldMap(string workItemTypeName, IFieldMap fieldToTagFieldMap)
        {
            if (string.IsNullOrEmpty(workItemTypeName))
            {
                throw new IndexOutOfRangeException("workItemTypeName on all fieldmaps must be set to at least '*'.");
            }
            if (!_fieldMaps.ContainsKey(workItemTypeName))
            {
                _fieldMaps.Add(workItemTypeName, new List<IFieldMap>());
            }
            _fieldMaps[workItemTypeName].Add(fieldToTagFieldMap);
        }

        public List<IFieldMap> GetFieldMappings(string witName)
        {
            if (_fieldMaps.TryGetValue("*", out List<IFieldMap> fieldMaps))
            {
                return fieldMaps;
            }
            else if (_fieldMaps.TryGetValue(witName, out fieldMaps))
            {
                return fieldMaps;
            }
            return [];
        }

        public void ApplyFieldMappings(WorkItemData source, WorkItemData target)
            => ProcessFieldMapList(source, target, GetFieldMappings(source.Fields["System.WorkItemType"].Value.ToString()));

        public void ApplyFieldMappings(WorkItemData target)
            => ApplyFieldMappings(target, target);

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
