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
    public class FieldValueMap : FieldMapBase
    {
        private FieldValueMapConfig config;

        public FieldValueMap(FieldValueMapConfig config)
        {
            this.config = config;
        }

        internal override void InternalExecute(WorkItem source, WorkItem target)
        {
                if (source.Fields.Contains(config.sourceField))
                {
                    // to tag
                    string value = (string)source.Fields[config.sourceField].Value;
                    if (config.valueMapping.ContainsKey(value))
                    {
                        target.Fields[config.targetField].Value = config.valueMapping[value];
                        Trace.WriteLine(string.Format("  [UPDATE] field value mapped {0}:{1} to {2}:{3}", source.Id, config.sourceField, target.Id, config.targetField));
                    }
                }

        }
    }
}
