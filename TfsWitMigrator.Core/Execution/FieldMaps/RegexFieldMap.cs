using System;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using VSTS.DataBulkEditor.Engine.ComponentContext;
using System.Text.RegularExpressions;
using System.Diagnostics;
using VSTS.DataBulkEditor.Engine.Configuration.FieldMap;

namespace VSTS.DataBulkEditor.Engine
{
    public class RegexFieldMap : IFieldMap
    {
        private RegexFieldMapConfig config;

        public RegexFieldMap(RegexFieldMapConfig config)
        {
            this.config = config;
        }

        public void Execute(WorkItem source, WorkItem target)
        {
            if (source.Fields.Contains(config.sourceField) && !string.IsNullOrEmpty(source.Fields[config.sourceField].Value.ToString()) && target.Fields.Contains(config.targetField))
            {
                if (Regex.IsMatch((string)source.Fields[config.sourceField].Value, config.pattern))
                {
                    target.Fields[config.targetField].Value = Regex.Replace((string)source.Fields[config.sourceField].Value, config.pattern, config.replacement);
                    Trace.WriteLine(string.Format("  [UPDATE] field tagged {0}:{1} to {2}:{3} with regex pattern of {4} resulting in {5}", source.Id, config.sourceField, target.Id, config.targetField, config.pattern, target.Fields[config.targetField].Value));
                }
            }
        }
    }
}