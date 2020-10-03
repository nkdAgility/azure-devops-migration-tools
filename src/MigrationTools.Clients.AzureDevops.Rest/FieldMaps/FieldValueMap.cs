using System;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using MigrationTools.Configuration.FieldMap;

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