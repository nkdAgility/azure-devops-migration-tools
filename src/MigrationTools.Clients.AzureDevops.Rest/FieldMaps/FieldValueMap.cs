using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.ApplicationInsights;
using MigrationTools.Configuration.FieldMap;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;

namespace MigrationTools.Clients.AzureDevops.Rest.FieldMaps
{
    public class FieldValueMap : FieldMapBase
    {
        private FieldValueMapConfig Config { get { return (FieldValueMapConfig)_Config; } }

        public override string MappingDisplayName => $"{Config.sourceField} {Config.targetField}";

        internal override void InternalExecute(WorkItem source, WorkItem target)
        {
             throw new NotImplementedException();

        }
    }
}
