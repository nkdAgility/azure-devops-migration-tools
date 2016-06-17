using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System.Diagnostics;
using Microsoft.ApplicationInsights;

namespace VSTS.DataBulkEditor.Engine.ComponentContext
{
    public class FieldMergeMap : FieldMapBase
    {
        private string sourceField1;
        private string sourceField2;
        private string targetField;
        private string format;

        public FieldMergeMap(string source1, string source2, string targetField, string format)
        {
            this.sourceField1 = source1;
            this.sourceField2 = source2;
            this.targetField = targetField;
            this.format = format;
        }

        internal override void InternalExecute(WorkItem source, WorkItem target)
        {
            if (source.Fields.Contains(sourceField1) && source.Fields.Contains(sourceField2))
            {
                  target.Fields[targetField].Value = string.Format(format, (string)source.Fields[sourceField1].Value, (string)source.Fields[sourceField2].Value);
                  Trace.WriteLine(string.Format("  [UPDATE] field merged {0}:{1}+{2} to {3}:{4}", source.Id, sourceField1, sourceField2, target.Id, targetField));
            }
        }
    }
}
