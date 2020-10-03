using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using MigrationTools.Configuration.FieldMap;
using MigrationTools.Configuration;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;

namespace MigrationTools.Clients.AzureDevops.Rest.FieldMaps
{
    public class FieldToFieldMap : FieldMapBase
    {
        private FieldtoFieldMapConfig Config { get { return (FieldtoFieldMapConfig)_Config; } }

        public override void Configure(IFieldMapConfig config)
        {
            base.Configure(config);

        }

        public override string MappingDisplayName => $"{Config.sourceField} {Config.targetField}";

        internal override void InternalExecute(WorkItem source, WorkItem target)
        {
            throw new NotImplementedException();
        }
    }
}
