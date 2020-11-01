using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using MigrationTools.Configuration;
using MigrationTools.Configuration.FieldMap;

namespace MigrationTools.FieldMaps.AzureDevops.ObjectModel
{
    public class FieldToTagFieldMap : FieldMapBase
    {
        public FieldToTagFieldMap(ILogger<FieldToTagFieldMap> logger) : base(logger)
        {
        }

        public override string MappingDisplayName => Config.sourceField;
        private FieldtoTagMapConfig Config { get { return (FieldtoTagMapConfig)_Config; } }

        public override void Configure(IFieldMapConfig config)
        {
            base.Configure(config);
        }

        internal override void InternalExecute(WorkItem source, WorkItem target)
        {
            if (source.Fields.Contains(this.Config.sourceField))
            {
                List<string> newTags = target.Tags.Split(char.Parse(@";")).ToList();
                // to tag
                if (source.Fields[this.Config.sourceField].Value != null)
                {
                    string value = source.Fields[this.Config.sourceField].Value.ToString();
                    if (!string.IsNullOrEmpty(value))
                    {
                        if (string.IsNullOrEmpty(Config.formatExpression))
                        {
                            newTags.Add(value);
                        }
                        else
                        {
                            newTags.Add(string.Format(Config.formatExpression, value));
                        }
                        target.Tags = string.Join(";", newTags.ToArray());
                        Log.LogDebug("FieldToTagFieldMap: [UPDATE] field tagged {0}:{1} to {2}:Tag with foramt of {3}", source.Id, Config.sourceField, target.Id, Config.formatExpression);
                    }
                }
            }
        }
    }
}