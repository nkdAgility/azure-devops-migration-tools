using System;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using VstsSyncMigrator.Engine.ComponentContext;
using System.Text.RegularExpressions;
using System.Diagnostics;
using VstsSyncMigrator.Engine.Configuration.FieldMap;

namespace VstsSyncMigrator.Engine
{
    public class RegexFieldMap : IFieldMap
    {
        private RegexFieldMapConfig config;

        public RegexFieldMap(RegexFieldMapConfig config)
        {
            this.config = config;
        }

        public string Name
        {
            get
            {
                return "RegexFieldMap";
            }
        }

        public string MappingDisplayName => $"{config.sourceField} {config.targetField}";

        public void Execute(WorkItem source, WorkItem target)
        {
            if (source.Fields.Contains(config.sourceField) && source.Fields[config.sourceField].Value != null && target.Fields.Contains(config.targetField))
            {
                if (Regex.IsMatch(source.Fields[config.sourceField].Value.ToString(), config.pattern))
                {
                    target.Fields[config.targetField].Value = Regex.Replace(source.Fields[config.sourceField].Value.ToString(), config.pattern, config.replacement);
                    Trace.WriteLine(string.Format("  [UPDATE] field tagged {0}:{1} to {2}:{3} with regex pattern of {4} resulting in {5}", source.Id, config.sourceField, target.Id, config.targetField, config.pattern, target.Fields[config.targetField].Value));
                }
            }
        }
    }
}