using System;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using MigrationTools._EngineV1.Configuration;
using MigrationTools._EngineV1.Configuration.FieldMap;

namespace MigrationTools.Clients.AzureDevops.Rest.FieldMaps
{
    public class FieldMergeMap : FieldMapBase
    {
        private FieldMergeMapOptions Config { get { return (FieldMergeMapOptions)_Config; } }

        public override void Configure(IFieldMapOptions config)
        {
            base.Configure(config);
          // Not implemented
        }

        public override string MappingDisplayName => $"Notimplemented";

        internal override void InternalExecute(WorkItem source, WorkItem target)
        {
            throw new NotImplementedException();
        }
    }
}