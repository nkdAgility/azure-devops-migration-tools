using System;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using MigrationTools._EngineV1.Configuration;
using MigrationTools.Tools;
using MigrationTools.Tools.Infrastructure;

namespace MigrationTools.Clients.AzureDevops.Rest.FieldMaps
{
    public class TreeToTagFieldMap : FieldMapBase
    {
        private TreeToTagMapOptions Config { get { return (TreeToTagMapOptions)_Config; } }

        public override void Configure(IFieldMapOptions config)
        {
            base.Configure(config);
        }

        public override string MappingDisplayName => string.Empty;

        internal override void InternalExecute(WorkItem source, WorkItem target)
        {
            throw new NotImplementedException();
            //List<string> newTags = target.Tags.Split(char.Parse(@";")).ToList();

            //string value;

            //if (Config.timeTravel > 0)
            //{
            //    value = (string)source.Revisions[source.Revision - Config.timeTravel].Fields["System.AreaPath"].Value;
            //}
            //else
            //{
            //    value = source.AreaPath;
            //}

            //List<string> bits = new List<string>(value.Split(char.Parse(@"\"))).Skip(Config.toSkip).ToList();
            //target.Tags = string.Join(";", newTags.Union(bits).ToArray());
        }
    }
}