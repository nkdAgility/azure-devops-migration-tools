using System;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using MigrationTools.Configuration;
using MigrationTools.Configuration.FieldMap;

namespace MigrationTools.Clients.AzureDevops.ObjectModel.FieldMaps
{
    public class FieldValuetoTagMap : FieldMapBase
    {
        public FieldValuetoTagMap(ILogger<FieldValuetoTagMap> logger) : base(logger)
        {
        }

        public override string MappingDisplayName => $"{Config.sourceField}";
        private FieldValuetoTagMapConfig Config { get { return (FieldValuetoTagMapConfig)_Config; } }

        public override void Configure(IFieldMapConfig config)
        {
            base.Configure(config);
        }

        internal override void InternalExecute(WorkItem source, WorkItem target)
        {
            if (source.Fields.Contains(Config.sourceField))
            {
                // parse existing tags entry
                var tags = target.Tags
                    .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                    .ToList();

                // only proceed if value is available
                var value = source.Fields[Config.sourceField].Value;
                var match = false;
                if (value != null)
                {
                    if (!string.IsNullOrEmpty(Config.pattern))
                    {
                        // regular expression matching is being used
                        match = Regex.IsMatch(value.ToString(), Config.pattern);
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
                    var newTag = string.IsNullOrEmpty(Config.formatExpression) ? value.ToString() : string.Format(Config.formatExpression, value);
                    if (!string.IsNullOrWhiteSpace(newTag))
                        tags.Add(newTag);

                    // rewrite tag values if changed
                    var newTags = string.Join(";", tags.Distinct());
                    if (newTags != target.Tags)
                    {
                        target.Tags = newTags;
                        Log.LogDebug("FieldValuetoTagMap: [UPDATE] field tagged {0}:{1} to {2}:Tag with format of {3}", source.Id, Config.sourceField, target.Id, Config.formatExpression);
                    }
                }
            }
        }
    }
}