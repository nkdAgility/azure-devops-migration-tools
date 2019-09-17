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
    public class FieldMergeMap : FieldMapBase
    {
        private FieldMergeMapConfig config;

        public FieldMergeMap(FieldMergeMapConfig config)
        {
            this.config = config;

            if(string.IsNullOrWhiteSpace(this.config.doneMatch))
            {
                throw new ArgumentNullException($"The {nameof(config.doneMatch)} configuration parameter cannot be null or empty.");
            }
        }

        public override string MappingDisplayName => $"{config.sourceField1}/{config.sourceField2} -> {config.targetField}";

        internal override void InternalExecute(WorkItem source, WorkItem target)
        {
            if (source.Fields.Contains(config.sourceField1) && source.Fields.Contains(config.sourceField2))
            {
                if (source.Fields[config.sourceField1].Value != null)
                {
                    if (target.Fields[config.targetField].Value.ToString().Contains(config.doneMatch))
                    {
                        Trace.WriteLine(string.Format("  [SKIP] field already merged {0}:{1}+{2} to {3}:{4}", source.Id, config.sourceField1, config.sourceField2, target.Id, config.targetField));
                    } else {
                        var val1 = source.Fields[config.sourceField1].Value != null ? source.Fields[config.sourceField1].Value.ToString() : string.Empty;
                        var val2 = source.Fields[config.sourceField2].Value != null ? source.Fields[config.sourceField2].Value.ToString() : string.Empty;

                        target.Fields[config.targetField].Value = string.Format(config.formatExpression, val1, val2) + config.doneMatch;
                        Trace.WriteLine(string.Format("  [UPDATE] field merged {0}:{1}+{2} to {3}:{4}", source.Id, config.sourceField1, config.sourceField2, target.Id, config.targetField));
                    }
                }
            }
        }
    }
}
