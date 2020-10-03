using System;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using MigrationTools.Configuration;
using MigrationTools.Configuration.FieldMap;

namespace MigrationTools.Clients.AzureDevops.ObjectModel.FieldMaps
{
    public class FieldLiteralMap : FieldMapBase
    {
        public FieldLiteralMap(ILogger<FieldLiteralMap> logger) : base(logger)
        {
        }

        private FieldLiteralMapConfig Config { get { return (FieldLiteralMapConfig)_Config; } }

        public override void Configure(IFieldMapConfig config)
        {
            base.Configure(config);

            if (Config.targetField == null)
            {
                throw new ArgumentNullException($"The target field `{Config.targetField}` must be specified. Please use diferent fields.");
            }
        }

        public override string MappingDisplayName => $"{Config.value} -> {Config.targetField}";

        internal override void InternalExecute(WorkItem source, WorkItem target)
        {
            target.Fields[Config.targetField].Value = Config.value;
        }
    }
}