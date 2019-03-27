using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System.Diagnostics;
using Microsoft.ApplicationInsights;
using VstsSyncMigrator.Engine.Configuration.FieldMap;

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
                string sourceValue = source.Fields[config.sourceField].Value != null 
                    ? source.Fields[config.sourceField].Value.ToString()
                    : null;

                if (sourceValue != null && config.valueMapping.ContainsKey(sourceValue))
                {
                    target.Fields[config.targetField].Value = config.valueMapping[sourceValue];
                    Trace.WriteLine($"  [UPDATE] field value mapped {source.Id}:{config.sourceField} to {target.Id}:{config.targetField}");
                }
                else if (sourceValue != null && !string.IsNullOrEmpty(config.defaultValue))
                {
                    target.Fields[config.targetField].Value = config.defaultValue;
                    Trace.WriteLine($"  [UPDATE] field set to default value {source.Id}:{config.sourceField} to {target.Id}:{config.targetField}");
                }
            }

        }

        internal override void InternalExecute(FieldCollection source, FieldCollection target)
        {
            if (source.Contains(config.sourceField))
            {
                string sourceValue = source[config.sourceField].Value != null
                    ? source[config.sourceField].Value.ToString()
                    : null;

                if (sourceValue != null && config.valueMapping.ContainsKey(sourceValue))
                {
                    target[config.targetField].Value = config.valueMapping[sourceValue];
                    Trace.WriteLine($"  [UPDATE] field value mapped {config.sourceField}:{sourceValue} to {config.targetField}{target[config.targetField].Value}");
                }
                else if (sourceValue != null && !string.IsNullOrEmpty(config.defaultValue))
                {
                    target[config.targetField].Value = config.defaultValue;
                    Trace.WriteLine($"  [UPDATE] field set to default value {config.sourceField} to {config.targetField}");
                }
            }

        }
    }
}
