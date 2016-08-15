using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System.Diagnostics;
using Microsoft.ApplicationInsights;
using VSTS.DataBulkEditor.Engine.Configuration.FieldMap;

namespace VSTS.DataBulkEditor.Engine.ComponentContext
{
    public class FieldMergeMap : FieldMapBase
    {
        private FieldMergeMapConfig config;

        public FieldMergeMap(FieldMergeMapConfig config)
        {
            this.config = config;
        }

        internal override void InternalExecute(WorkItem source, WorkItem target)
        {
            if (source.Fields.Contains(config.sourceField1) && source.Fields.Contains(config.sourceField2))
            {
                if (source.Fields[config.sourceField1].Value != null)
                {
                    target.Fields[config.targetField].Value = string.Format(config.formatExpression, (string)source.Fields[config.sourceField1].Value.ToString(), (string)source.Fields[config.sourceField2].Value.ToString());
                    Trace.WriteLine(string.Format("  [UPDATE] field merged {0}:{1}+{2} to {3}:{4}", source.Id, config.sourceField1, config.sourceField2, target.Id, config.targetField));
                }
            }
        }
    }
}
