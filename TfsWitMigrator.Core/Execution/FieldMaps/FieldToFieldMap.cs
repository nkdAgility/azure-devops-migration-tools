using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System.Diagnostics;
using VSTS.DataBulkEditor.Engine.Configuration.FieldMap;

namespace VSTS.DataBulkEditor.Engine.ComponentContext
{
    public class FieldToFieldMap : FieldMapBase
    {
        private FieldtoFieldMapConfig config;
        private string targetField;

        public FieldToFieldMap(FieldtoFieldMapConfig config)
        {
            this.config = config;
        }

        internal override void InternalExecute(WorkItem source, WorkItem target)
        {
            if (source.Fields.Contains(config.sourceField) && target.Fields.Contains(targetField))
            {
                target.Fields[targetField].Value = source.Fields[config.sourceField].Value;
                Trace.WriteLine(string.Format("  [UPDATE] field mapped {0}:{1} to {2}:{3}", source.Id, config.sourceField, target.Id, targetField));
            }
        }
    }
}
