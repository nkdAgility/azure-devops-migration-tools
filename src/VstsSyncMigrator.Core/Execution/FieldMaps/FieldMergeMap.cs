using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System.Diagnostics;
using Microsoft.ApplicationInsights;
using AzureDevOpsMigrationTools.Core.Configuration.FieldMap;

namespace VstsSyncMigrator.Engine.ComponentContext
{
    public class FieldMergeMap : FieldMapBase
    {
        private FieldMergeMapConfig config;

        public FieldMergeMap(FieldMergeMapConfig config)
        {
            this.config = config;
            if (config.targetField == config.sourceField2)
            {
                throw new ArgumentNullException($"The source field `{config.sourceField2}` can not match target field `{config.targetField}`. Please use diferent fields.");
            }
        }

        public override string MappingDisplayName => $"{config.sourceField1}/{config.sourceField2} -> {config.targetField}";

        internal override void InternalExecute(WorkItem source, WorkItem target)
        {
            if (source.Fields.Contains(config.sourceField1) && source.Fields.Contains(config.sourceField2))
            {
                var val1 = source.Fields[config.sourceField1].Value != null ? source.Fields[config.sourceField1].Value.ToString() : string.Empty;
                var val2 = source.Fields[config.sourceField2].Value != null ? source.Fields[config.sourceField2].Value.ToString() : string.Empty;
                var valT = target.Fields[config.targetField].Value != null ? target.Fields[config.targetField].Value.ToString() : string.Empty;
                var newValT = string.Format(config.formatExpression, val1, val2);
                if (valT.Contains(val2))
                {
                    Trace.WriteLine(string.Format("  [SKIP] field already merged {0}:{1}+{2} to {3}:{4}", source.Id, config.sourceField1, config.sourceField2, target.Id, config.targetField));
                } else if (valT.Equals(newValT))
                    {
                    Trace.WriteLine(string.Format("  [SKIP] field already merged {0}:{1}+{2} to {3}:{4}", source.Id, config.sourceField1, config.sourceField2, target.Id, config.targetField));
                } else
                {
                    target.Fields[config.targetField].Value = newValT;
                    Trace.WriteLine(string.Format("  [UPDATE] field merged {0}:{1}+{2} to {3}:{4}", source.Id, config.sourceField1, config.sourceField2, target.Id, config.targetField));
                }
            }
        }
    }
}
