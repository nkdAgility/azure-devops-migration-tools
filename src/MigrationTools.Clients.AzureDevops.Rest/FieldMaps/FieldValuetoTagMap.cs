using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using MigrationTools.Configuration;
using MigrationTools.Configuration.FieldMap;

namespace MigrationTools.Clients.AzureDevops.Rest.FieldMaps
{

    public class FieldValuetoTagMap : FieldMapBase
    {

        private FieldValuetoTagMapConfig Config { get { return (FieldValuetoTagMapConfig)_Config; } }

        public override void Configure(IFieldMapConfig config)
        {
            base.Configure(config);
        }
        public override string MappingDisplayName => $"{Config.sourceField}";

        internal override void InternalExecute(WorkItem source, WorkItem target)
        {
            throw new NotImplementedException();
          
        }
    }

}