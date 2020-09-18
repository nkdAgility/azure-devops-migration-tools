using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System.Diagnostics;
using MigrationTools.Core.Configuration.FieldMap;

namespace VstsSyncMigrator.Engine.ComponentContext
{
    public class FieldToFieldMap : FieldMapBase
    {
        private FieldtoFieldMapConfig config;

        public FieldToFieldMap(FieldtoFieldMapConfig config)
        {
            this.config = config;
        }

        public override string MappingDisplayName => $"{config.sourceField} {config.targetField}";

        internal override void InternalExecute(WorkItem source, WorkItem target)
        {
            if (source.Fields.Contains(config.sourceField) && target.Fields.Contains(config.targetField))
            {
                var value = source.Fields[config.sourceField].Value;
                if ((value as string is null || value as string == "") && config.defaultValue != null)
                {
                    value = config.defaultValue;
                }
                target.Fields[config.targetField].Value = value;
                Trace.WriteLine(string.Format("  [UPDATE] field mapped {0}:{1} to {2}:{3}", source.Id, config.sourceField, target.Id, config.targetField));
            }
        }
    }
}
