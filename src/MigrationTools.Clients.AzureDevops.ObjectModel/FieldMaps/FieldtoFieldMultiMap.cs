using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using MigrationTools._EngineV1.Configuration;
using MigrationTools._EngineV1.Configuration.FieldMap;

namespace MigrationTools.FieldMaps.AzureDevops.ObjectModel
{
    public class FieldtoFieldMultiMap : FieldMapBase
    {
        public FieldtoFieldMultiMap(ILogger<FieldtoFieldMultiMap> logger, ITelemetryLogger telemetryLogger) : base(logger, telemetryLogger)
        {
        }

        public override string MappingDisplayName => string.Empty;
        private FieldtoFieldMultiMapConfig Config { get { return (FieldtoFieldMultiMapConfig)_Config; } }

        public override void Configure(IFieldMapConfig config)
        {
            base.Configure(config);
        }

        internal override void InternalExecute(WorkItem source, WorkItem target)
        {
            if (fieldsExist(Config.SourceToTargetMappings, source, target))
                mapFields(Config.SourceToTargetMappings, source, target);
            else
                Log.LogDebug("FieldtoFieldMultiMap:  [SKIPPED] Not all source and target fields exist.");
        }

        private bool fieldsExist(Dictionary<string, string> fieldMap, WorkItem source, WorkItem target)
        {
            bool exists = true;
            foreach (var map in fieldMap)
            {
                if (!source.Fields.Contains(map.Key))
                {
                    exists = false;
                    Log.LogDebug("FieldtoFieldMultiMap:  Configured Field {Field} does not exist in the source on FieldtoFieldMultiMap", map.Key);
                }
                if (!target.Fields.Contains(map.Value))
                {
                    exists = false;
                    Log.LogDebug("FieldtoFieldMultiMap:  Configured Field {Field} does not exist in the target on FieldtoFieldMultiMap", map.Key);
                }
            }
            return exists;
        }

        private void mapFields(Dictionary<string, string> fieldMap, WorkItem source, WorkItem target)
        {
            foreach (var map in fieldMap)
            {
                target.Fields[map.Value].Value = source.Fields[map.Key].Value;
                Log.LogDebug("FieldtoFieldMultiMap:  [UPDATE] field mapped {0}:{1} to {2}:{3}", source.Id, map.Key, target.Id, map.Value);
            }
        }
    }
}