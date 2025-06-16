//New COmment
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using MigrationTools._EngineV1.Configuration;
using MigrationTools.Tools;

namespace MigrationTools.FieldMaps.AzureDevops.ObjectModel
{
    /// <summary>
    /// Skips field mapping for a specific target field, effectively leaving the field unchanged during migration.
    /// </summary>
    public class FieldSkipMap : FieldMapBase
    {
        /// <summary>
        /// Initializes a new instance of the FieldSkipMap class.
        /// </summary>
        /// <param name="logger">Logger for the field map operations</param>
        /// <param name="telemetryLogger">Telemetry logger for tracking operations</param>
        public FieldSkipMap(ILogger<FieldSkipMap> logger, ITelemetryLogger telemetryLogger) : base(logger, telemetryLogger)
        {
        }

        public override string MappingDisplayName => $"{Config.targetField}";
        private FieldSkipMapOptions Config { get { return (FieldSkipMapOptions)_Config; } }

        internal override void InternalExecute(WorkItem source, WorkItem target)
        {
            if (!target.Fields.Contains(Config.targetField)) {
                Log.LogDebug("FieldSkipMap: target field does not exist '{0}' for '{1}'", Config.targetField, target.Type.Name);
                return;
            }
            var targetField = target.Fields[Config.targetField];
            if (targetField.IsLimitedToAllowedValues && targetField.AllowedValues.Count > 0 && !targetField.AllowedValues.Contains(targetField.OriginalValue as string)) {
                Log.LogDebug("FieldSkipMap: target field '{0}' is limited to allowed values list: '{1}'", Config.targetField, string.Join("', '", targetField.AllowedValues));
                return;
            }
            if (!targetField.IsEditable) {
                Log.LogDebug("FieldSkipMap: target field '{0}' is not editable!", Config.targetField);
                return;
            }
            if (targetField.IsRequired) {
                Log.LogDebug("FieldSkipMap: target field '{0}' is required!", Config.targetField);
                return;
            }
            targetField.Value = targetField.OriginalValue;
            Log.LogDebug("FieldSkipMap: field mapped {0}:{1} to {2} blanked", source.Id, target.Id, this.Config.targetField);
        }
    }
}