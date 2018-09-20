using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System.Diagnostics;
using VstsSyncMigrator.Engine.Configuration.FieldMap;

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
                target.Fields[config.targetField].Value = source.Fields[config.sourceField].Value;
                Trace.WriteLine(string.Format("  [UPDATE] field mapped {0}:{1} to {2}:{3}", source.Id, config.sourceField, target.Id, config.targetField));
            }
        }
    }
}
