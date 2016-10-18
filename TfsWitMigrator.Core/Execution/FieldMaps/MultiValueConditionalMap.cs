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
    public class MultiValueConditionalMap : FieldMapBase
    {
        private MultiValueConditionalMapConfig config;

        public MultiValueConditionalMap(MultiValueConditionalMapConfig config)
        {
            this.config = config;
        }

        internal override void InternalExecute(WorkItem source, WorkItem target)
        {
            if (fieldsExist(config.sourceFieldsAndValues, source) && fieldsExist(config.targetFieldsAndValues, target))
            {
                if (fieldsValueMatch(config.sourceFieldsAndValues, source))
                {
                    fieldsUpdate(config.targetFieldsAndValues, target);
                }                
                Trace.WriteLine(string.Format("  [UPDATE] field mapped {0}:{1} to {2}:{3}", source.Id, config.sourceFieldsAndValues.Keys.ToString(), target.Id, config.targetFieldsAndValues.Keys.ToString());
            } else
            {
                Trace.WriteLine(string.Format("  [SKIPPED] Not all source and target fields exist", source.Id, config.sourceFieldsAndValues.Keys.ToString(), target.Id, config.targetFieldsAndValues.Keys.ToString()));
            }
        }

        private void fieldsUpdate(Dictionary<string, string> fieldAndValues, WorkItem workitem)
        {
            foreach (string field in fieldAndValues.Keys)
            {
                workitem.Fields[field].Value = fieldAndValues[field];
            }
        }

        private bool fieldsValueMatch(Dictionary<string, string> fieldAndValues, WorkItem workitem)
        {
            bool matches = true;
            foreach (string field in fieldAndValues.Keys)
            {
                if((string)workitem.Fields[field].Value != fieldAndValues[field])
                {
                    matches = false;
                }
            }
            return matches;
        }

        private bool fieldsExist(Dictionary<string, string> fieldsAndValues, WorkItem workitem)
        {
            bool exists = true;
            foreach (string field in fieldsAndValues.Keys)
            {
                if (!workitem.Fields.Contains(field))
                {
                    exists = false;
                }
            }
            return exists;
        }


    }
}
