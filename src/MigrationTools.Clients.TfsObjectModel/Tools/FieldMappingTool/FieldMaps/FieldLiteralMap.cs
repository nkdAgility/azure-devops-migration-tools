using System;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using MigrationTools._EngineV1.Configuration;
using MigrationTools.Tools;
using MigrationTools.Tools.Infrastructure;

namespace MigrationTools.FieldMaps.AzureDevops.ObjectModel
{
    /// <summary>
    /// Maps a literal (static) value to a target field, useful for setting constant values across all migrated work items.
    /// </summary>
    public class FieldLiteralMap : FieldMapBase
    {
        /// <summary>
        /// Initializes a new instance of the FieldLiteralMap class.
        /// </summary>
        /// <param name="logger">Logger for the field map operations</param>
        /// <param name="telemetryLogger">Telemetry logger for tracking operations</param>
        public FieldLiteralMap(ILogger<FieldLiteralMap> logger, ITelemetryLogger telemetryLogger) : base(logger, telemetryLogger)
        {
        }

        private FieldLiteralMapOptions Config { get { return (FieldLiteralMapOptions)_Config; } }

        /// <summary>
        /// Configures the field map with the specified options and validates required settings.
        /// </summary>
        /// <param name="config">The field map configuration options</param>
        /// <exception cref="ArgumentNullException">Thrown when the target field is not specified</exception>
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