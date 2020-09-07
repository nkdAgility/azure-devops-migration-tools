using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using AzureDevOpsMigrationTools.Core.Configuration.FieldMap;

namespace VstsSyncMigrator.Engine.ComponentContext
{
    public class FieldtoFieldMultiMap : FieldMapBase
    {
        private FieldtoFieldMultiMapConfig config;

        public FieldtoFieldMultiMap(FieldtoFieldMultiMapConfig config)
        {
            this.config = config;
        }

        public override string MappingDisplayName => string.Empty;

        internal override void InternalExecute(WorkItem source, WorkItem target)
        {
            if (fieldsExist(config.SourceToTargetMappings, source, target))
                mapFields(config.SourceToTargetMappings, source, target);
            else
                Trace.WriteLine("  [SKIPPED] Not all source and target fields exist.");
        }

        private bool fieldsExist(Dictionary<string, string> fieldMap, WorkItem source, WorkItem target)
        {
            bool exists = true;
            foreach (var map in fieldMap)
                if (!source.Fields.Contains(map.Key) || !target.Fields.Contains(map.Value))
                    exists = false;

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
