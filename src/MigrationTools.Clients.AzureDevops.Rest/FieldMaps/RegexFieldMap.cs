using System;
using System.Text.RegularExpressions;
using System.Diagnostics;
using MigrationTools.Configuration.FieldMap;
using MigrationTools.Configuration;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;

namespace MigrationTools.Clients.AzureDevops.Rest.FieldMaps
{
    public class RegexFieldMap : FieldMapBase
    {
        private RegexFieldMapConfig Config { get { return (RegexFieldMapConfig)_Config; } }

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