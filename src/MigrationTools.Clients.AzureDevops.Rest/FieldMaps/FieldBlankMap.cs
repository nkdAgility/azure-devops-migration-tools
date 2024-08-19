//New COmment
using System;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using MigrationTools.Tools.Infrastructure;
using MigrationTools.Tools;

namespace MigrationTools.Clients.AzureDevops.Rest.FieldMaps
{
    public class FieldSkipMap : FieldMapBase
    {
        private FieldSkipMapOptions Config { get { return (FieldSkipMapOptions)_Config; } }

        public override string MappingDisplayName => $"{Config.targetField}";

        internal override void InternalExecute(WorkItem source, WorkItem target)
        {
            throw new NotImplementedException();
        }
    }
}