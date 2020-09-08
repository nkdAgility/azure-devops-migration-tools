using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System.Diagnostics;
using Microsoft.ApplicationInsights;
using MigrationTools.Core.Configuration.FieldMap;

namespace VstsSyncMigrator.Engine.ComponentContext
{
    public class FieldValueMap : FieldMapBase
    {
        private FieldValueMapConfig config;

        public FieldValueMap(FieldValueMapConfig config)
        {
            this.config = config;
        }

        public override string MappingDisplayName => $"{config.sourceField} {config.targetField}";

        internal override void InternalExecute(WorkItem source, WorkItem target)
        {
            if (source.Fields.Contains(config.sourceField))
            {
                var sourceVal = source.Fields[config.sourceField].Value;
                var t = target.Fields[config.targetField].FieldDefinition.SystemType;

                if (sourceVal is null && config.valueMapping.ContainsKey("null"))
                {
                    target.Fields[config.targetField].Value = Convert.ChangeType(config.valueMapping["null"], t);
                    Trace.WriteLine($"  [UPDATE] field value mapped {source.Id}:{config.sourceField} to {target.Id}:{config.targetField}");
                }
                else if (sourceVal != null && config.valueMapping.ContainsKey(sourceVal.ToString()))
                {
                    target.Fields[config.targetField].Value = Convert.ChangeType(config.valueMapping[sourceVal.ToString()], t);
                    Trace.WriteLine($"  [UPDATE] field value mapped {source.Id}:{config.sourceField} to {target.Id}:{config.targetField}");
                }
                else if (sourceVal != null && !string.IsNullOrWhiteSpace(config.defaultValue))
                {
                    target.Fields[config.targetField].Value = Convert.ChangeType(config.defaultValue, t);
                    Trace.WriteLine($"  [UPDATE] field set to default value {source.Id}:{config.sourceField} to {target.Id}:{config.targetField}");
                }
            }
        }
    }
}
