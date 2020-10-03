using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.ApplicationInsights;
using MigrationTools.Configuration.FieldMap;
using MigrationTools.Configuration;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;

namespace MigrationTools.Clients.AzureDevops.Rest.FieldMaps
{
    public class FieldMergeMap : FieldMapBase
    {
        private FieldMergeMapConfig Config { get { return (FieldMergeMapConfig)_Config; } }

        public override void Configure(IFieldMapConfig config)
        {
            base.Configure(config);
            if (Config.targetField == Config.sourceField2)
            {
                throw new ArgumentNullException($"The source field `{Config.sourceField2}` can not match target field `{Config.targetField}`. Please use diferent fields.");
            }
        }

        public override string MappingDisplayName => $"{Config.sourceField1}/{Config.sourceField2} -> {Config.targetField}";

        internal override void InternalExecute(WorkItem source, WorkItem target)
        {
            throw new NotImplementedException();
        }
    }
}
