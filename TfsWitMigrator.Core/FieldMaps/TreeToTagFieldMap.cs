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

        public TreeToTagFieldMap(int toSkip)
        {
            this.toSkip = toSkip;
        }

        public void Execute(WorkItem source, WorkItem target)
        {
            List<string> newTags = target.Tags.Split(char.Parse(@";")).ToList();
            List<string> bits = new List<string>(source.AreaPath.Split(char.Parse(@"\"))).Skip(toSkip).ToList();
            target.Tags = string.Join(";", newTags.Union(bits).ToArray());
        }
    }
}