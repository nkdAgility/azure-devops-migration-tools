using System;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using VSTS.DataBulkEditor.Engine.ComponentContext;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace VSTS.DataBulkEditor.Engine
{
    public class FieldToTagFieldMap : IFieldMap
    {
        private string formatExpression;
        private string sourceField;

        public FieldToTagFieldMap(string sourceField, string formatExpression)
        {
            this.sourceField = sourceField;
            this.formatExpression = formatExpression;
        }

        public void Execute(WorkItem source, WorkItem target)
        {
            if (source.Fields.Contains(sourceField))
            {
                List<string> newTags = target.Tags.Split(char.Parse(@";")).ToList();
                // to tag
                string value = (string)source.Fields[sourceField].Value;
                if (string.IsNullOrEmpty(formatExpression))
                {
                    newTags.Add(value);
                }
                else
                {
                    newTags.Add(string.Format(formatExpression, value));
                }

                target.Tags = string.Join(";", newTags.ToArray());
                Trace.WriteLine(string.Format("  [UPDATE] field tagged {0}:{1} to {2}:Tag with foramt of {3}", source.Id, sourceField, target.Id, formatExpression));
            }
        }
    }
}