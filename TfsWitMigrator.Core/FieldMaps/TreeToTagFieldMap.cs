using System;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using TfsWitMigrator.Core.ComponentContext;
using System.Collections.Generic;
using System.Linq;

namespace TfsWitMigrator.Core
{
    public class TreeToTagFieldMap : IFieldMap
    {
        private int toSkip;
        private int timeTravel;

        public TreeToTagFieldMap(int toSkip, int timeTravel = 0)
        {
            this.timeTravel = timeTravel;
        }

        public void Execute(WorkItem source, WorkItem target)
        {

            List<string> newTags = target.Tags.Split(char.Parse(@";")).ToList();

            string value;

            if (timeTravel > 0)
            {
                value = (string)source.Revisions[source.Revision - timeTravel].Fields["System.AreaPath"].Value;
            }
            else
            {
                value = source.AreaPath;
            }

            List<string> bits = new List<string>(value.Split(char.Parse(@"\"))).Skip(toSkip).ToList();
            target.Tags = string.Join(";", newTags.Union(bits).ToArray());
        }
    }
}