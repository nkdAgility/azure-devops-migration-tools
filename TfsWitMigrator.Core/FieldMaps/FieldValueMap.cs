using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System.Diagnostics;
using Microsoft.ApplicationInsights;

namespace _VSTS.DataBulkEditor.Engine.ComponentContext
{
    public class FieldValueMap : FieldMapBase
    {
        private string sourceField;
        private string targetField;
        private Dictionary<string, string> valueMapping;

        public FieldValueMap(string sourceField, string targetField, Dictionary<string, string> valueMapping)
        {
            this.sourceField = sourceField;
            this.targetField = targetField;
            this.valueMapping = valueMapping;
        }



        internal override void InternalExecute(WorkItem source, WorkItem target)
        {
                if (source.Fields.Contains(sourceField))
                {
                    // to tag
                    string value = (string)source.Fields[sourceField].Value;
                    if (valueMapping.ContainsKey(value))
                    {
                        target.Fields[targetField].Value = valueMapping[value];
                        Trace.WriteLine(string.Format("  [UPDATE] field value mapped {0}:{1} to {2}:{3}", source.Id, sourceField, target.Id, targetField));
                    }
                }

        }
    }
}
