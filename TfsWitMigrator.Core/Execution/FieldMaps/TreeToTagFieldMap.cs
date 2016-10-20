using System;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using VSTS.DataBulkEditor.Engine.ComponentContext;
using System.Collections.Generic;
using System.Linq;
using VSTS.DataBulkEditor.Engine.Configuration.FieldMap;

namespace VSTS.DataBulkEditor.Engine
{
    public class TreeToTagFieldMap : IFieldMap
    {
        private TreeToTagMapConfig config;

        public TreeToTagFieldMap(TreeToTagMapConfig config)
        {
            this.config = config;
        }
        public string Name
        {
            get
            {
                return "TreeToTagFieldMap";
            }
        }
        public void Execute(WorkItem source, WorkItem target)
        {

            List<string> newTags = target.Tags.Split(char.Parse(@";")).ToList();

            string value;

            if (config.timeTravel > 0)
            {
                value = (string)source.Revisions[source.Revision - config.timeTravel].Fields["System.AreaPath"].Value;
            }
            else
            {
                value = source.AreaPath;
            }

            List<string> bits = new List<string>(value.Split(char.Parse(@"\"))).Skip(config.toSkip).ToList();
            target.Tags = string.Join(";", newTags.Union(bits).ToArray());
        }
    }
}