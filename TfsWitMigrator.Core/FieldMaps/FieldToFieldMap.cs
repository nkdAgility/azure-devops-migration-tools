using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System.Diagnostics;

namespace VSTS.DataBulkEditor.Engine.ComponentContext
{
    public class FieldToFieldMap : FieldMapBase
    {
        private string sourceField;
        private string targetField;

        public FieldToFieldMap(string sourceField, string targetField)
        {
            this.sourceField = sourceField;
            this.targetField = targetField;
        }

        internal override void InternalExecute(WorkItem source, WorkItem target)
        {
            if (source.Fields.Contains(sourceField) && target.Fields.Contains(targetField))
            {
                target.Fields[targetField].Value = source.Fields[sourceField].Value;
                Trace.WriteLine(string.Format("  [UPDATE] field mapped {0}:{1} to {2}:{3}", source.Id, sourceField, target.Id, targetField));
            }
        }
    }
}
