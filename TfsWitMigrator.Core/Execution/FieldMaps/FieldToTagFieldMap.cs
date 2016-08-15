using System;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using VSTS.DataBulkEditor.Engine.ComponentContext;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using VSTS.DataBulkEditor.Engine.Configuration.FieldMap;

namespace VSTS.DataBulkEditor.Engine
{
    public class FieldToTagFieldMap : IFieldMap
    {
        private FieldtoTagMapConfig config;
        private string sourceField;

        public FieldToTagFieldMap(FieldtoTagMapConfig config)
        {
            this.config = config;
        }

        public void Execute(WorkItem source, WorkItem target)
        {
            if (source.Fields.Contains(sourceField))
            {
                List<string> newTags = target.Tags.Split(char.Parse(@";")).ToList();
                // to tag
                if (source.Fields[sourceField].Value != null)
                {
                    string value = source.Fields[sourceField].Value.ToString();
                    if (string.IsNullOrEmpty(config.formatExpression))
                    {
                        newTags.Add(value);
                    }
                    else
                    {
                        newTags.Add(string.Format(config.formatExpression, value));
                    }

                    target.Tags = string.Join(";", newTags.ToArray());
                    Trace.WriteLine(string.Format("  [UPDATE] field tagged {0}:{1} to {2}:Tag with foramt of {3}", source.Id, sourceField, target.Id, config.formatExpression));
                }
                
            }
        }
    }
}