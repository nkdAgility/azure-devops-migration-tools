using System;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using MigrationTools.Tools;
using MigrationTools.Tools.Infrastructure;

namespace MigrationTools.Clients.AzureDevops.Rest.FieldMaps
{
    public class FieldValueMap : FieldMapBase
    {
        private FieldValueMapOptions Config { get { return (FieldValueMapOptions)_Config; } }

        public override string MappingDisplayName => $"{Config.sourceField} {Config.targetField}";

        internal override void InternalExecute(WorkItem source, WorkItem target)
        {
            throw new NotImplementedException();
        }
    }
}