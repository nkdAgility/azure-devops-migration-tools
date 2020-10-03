using System;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using MigrationTools.Configuration;
using MigrationTools.Configuration.FieldMap;

namespace MigrationTools.Clients.AzureDevops.Rest.FieldMaps
{
    public class FieldtoFieldMultiMap : FieldMapBase
    {
        private FieldtoFieldMultiMapConfig Config { get { return (FieldtoFieldMultiMapConfig)_Config; } }

        public override void Configure(IFieldMapConfig config)
        {
            base.Configure(config);
        }

        public override string MappingDisplayName => string.Empty;

        internal override void InternalExecute(WorkItem source, WorkItem target)
        {
            throw new NotImplementedException();
        }
    }
}