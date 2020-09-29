using System;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System.Collections.Generic;
using System.Linq;
using MigrationTools.Core.Configuration.FieldMap;
using MigrationTools.Core.Configuration;

namespace MigrationTools.Clients.AzureDevops.ObjectModel.FieldMaps
{
    public class TreeToTagFieldMap : FieldMapBase
    {

        private TreeToTagMapConfig Config { get { return (TreeToTagMapConfig)_Config; } }

        public override void Configure(IFieldMapConfig config)
        {
            base.Configure(config);
        }

        public override string MappingDisplayName => string.Empty;

        internal override void InternalExecute(WorkItem source, WorkItem target)
        {
            List<string> newTags = target.Tags.Split(char.Parse(@";")).ToList();

            string value;

            if (Config.timeTravel > 0)
            {
                value = (string)source.Revisions[source.Revision - Config.timeTravel].Fields["System.AreaPath"].Value;
            }
            else
            {
                value = source.AreaPath;
            }

            List<string> bits = new List<string>(value.Split(char.Parse(@"\"))).Skip(Config.toSkip).ToList();
            target.Tags = string.Join(";", newTags.Union(bits).ToArray());
        }
    }
}