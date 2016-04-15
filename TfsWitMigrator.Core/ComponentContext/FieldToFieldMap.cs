using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System.Diagnostics;

namespace TfsWitMigrator.Core.ComponentContext
{
    public class FieldToFieldMap : IFieldMap
    {
        private string sourceField;
        private string targetField;

        public FieldToFieldMap(string sourceField, string targetField)
        {
            this.sourceField = sourceField;
            this.targetField = targetField;
        }

        public void Execute(WorkItem source, WorkItem target)
        {
            if (source.Fields.Contains(sourceField))
            {
                // to tag
                string value = (string)source.Fields[sourceField].Value;
                target.Fields[targetField].Value = value;
                Trace.WriteLine(string.Format("  [UPDATE] field mapped {0}:{1} to {2}:{3}", source.Id, sourceField, target.Id, targetField ));
            }
        }
    }
}
