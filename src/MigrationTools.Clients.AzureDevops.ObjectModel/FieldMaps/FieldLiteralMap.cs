using System;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using MigrationTools._EngineV1.Configuration;
using MigrationTools._EngineV1.Configuration.FieldMap;

namespace MigrationTools.FieldMaps.AzureDevops.ObjectModel
{
    public class FieldLiteralMap : FieldMapBase
    {
        public FieldLiteralMap(ILogger<FieldLiteralMap> logger, ITelemetryLogger telemetryLogger) : base(logger, telemetryLogger)
        {
        }

        private FieldLiteralMapOptions Config { get { return (FieldLiteralMapOptions)_Config; } }

        public override void Configure(IFieldMapOptions config)
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