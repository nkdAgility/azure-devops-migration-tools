using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using MigrationTools.Configuration;
using MigrationTools.Configuration.FieldMap;
using Serilog;

namespace MigrationTools.Clients.AzureDevops.ObjectModel.FieldMaps
{
    public class FieldtoFieldMultiMap : FieldMapBase
    {
        public FieldtoFieldMultiMap(ILogger<FieldtoFieldMultiMap> logger) : base(logger)
        {
        }

        private FieldtoFieldMultiMapConfig Config { get { return (FieldtoFieldMultiMapConfig)_Config; } }

        public override void Configure(IFieldMapConfig config)
        {
            base.Configure(config);
        }

        public override string MappingDisplayName => string.Empty;

        internal override void InternalExecute(WorkItem source, WorkItem target)
        {
            if (fieldsExist(Config.SourceToTargetMappings, source, target))
                mapFields(Config.SourceToTargetMappings, source, target);
            else
                Log.Information("  [SKIPPED] Not all source and target fields exist.");
        }

        private bool fieldsExist(Dictionary<string, string> fieldMap, WorkItem source, WorkItem target)
        {
            bool exists = true;
            foreach (var map in fieldMap)
            {
                if (!source.Fields.Contains(map.Key))
                {
                    exists = false;
                    Log.Warning("Configured Field {Field} does not exist in the source on FieldtoFieldMultiMap", map.Key);
                }
                if (!target.Fields.Contains(map.Value))
                {
                    exists = false;
                    Log.Warning("Configured Field {Field} does not exist in the target on FieldtoFieldMultiMap", map.Key);
                }
            }
            return exists;
        }

        private void mapFields(Dictionary<string, string> fieldMap, WorkItem source, WorkItem target)
        {
            foreach (var map in fieldMap)
            {
                target.Fields[map.Value].Value = source.Fields[map.Key].Value;
                Trace.WriteLine(string.Format("  [UPDATE] field mapped {0}:{1} to {2}:{3}", source.Id, map.Key, target.Id, map.Value));
            }
        }
    }
}