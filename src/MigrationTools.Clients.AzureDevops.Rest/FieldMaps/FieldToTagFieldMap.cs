using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MigrationTools.Configuration.FieldMap;
using MigrationTools.Configuration;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;

namespace MigrationTools.Clients.AzureDevops.Rest.FieldMaps
{
    public class FieldToTagFieldMap : FieldMapBase
    {
        private FieldtoTagMapConfig Config { get { return (FieldtoTagMapConfig)_Config; } }

        public override void Configure(IFieldMapConfig config)
        {
            base.Configure(config);
        }

        public override string MappingDisplayName => Config.sourceField;

        internal override void InternalExecute(WorkItem source, WorkItem target)
        {
            throw new NotImplementedException();

        }
    }
}
