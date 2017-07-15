using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

using Microsoft.TeamFoundation.WorkItemTracking.Client;

using VstsSyncMigrator.Engine.ComponentContext;
using VstsSyncMigrator.Engine.Configuration.FieldMap;

namespace VstsSyncMigrator.Engine
{

    public class FieldValuetoTagMap : IFieldMap
    {

        FieldValuetoTagMapConfig config;

        public FieldValuetoTagMap(FieldValuetoTagMapConfig config)
        {
            this.config = config;
        }

        public string Name
        {
            get { return "FieldValuetoTagMap"; }
        }

        public void Execute(WorkItem source, WorkItem target)
        {
            if (source.Fields.Contains(config.sourceField))
            {
                // parse existing tags entry
                var tags = target.Tags
                    .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                    .ToList();

                // only proceed if value is available
                var value = source.Fields[config.sourceField].Value;
                var match = false;
                if (value != null)
                {
                    if (!string.IsNullOrEmpty(config.pattern))
                    {
                        // regular expression matching is being used
                        match = Regex.IsMatch(value.ToString(), config.pattern);
                    }
                    else
                    {
                        // always apply tag if value exists
                        match = true;
                    }
                }

                // add new tag if available
                if (match)
                {
                    // format or simple to string
                    var newTag = string.IsNullOrEmpty(config.formatExpression) ? value.ToString() : string.Format(config.formatExpression, value);
                    if (!string.IsNullOrWhiteSpace(newTag))
                        tags.Add(newTag);

                    // rewrite tag values if changed
                    var newTags = string.Join(";", tags.Distinct());
                    if (newTags != target.Tags)
                    {
                        target.Tags = newTags;
                        Trace.WriteLine(string.Format("  [UPDATE] field tagged {0}:{1} to {2}:Tag with format of {3}", source.Id, config.sourceField, target.Id, config.formatExpression));
                    }
                }
            }
        }

    }

}