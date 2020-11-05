using System;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using MigrationTools._EngineV1.Configuration;
using MigrationTools._EngineV1.Configuration.FieldMap;

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