//New COmment
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using MigrationTools._EngineV1.Configuration.FieldMap;

namespace MigrationTools.FieldMaps.AzureDevops.ObjectModel
{
    public class FieldClearMap : FieldMapBase
    {
        public FieldClearMap(ILogger<FieldSkipMap> logger, ITelemetryLogger telemetryLogger) : base(logger, telemetryLogger)
        {
        }

        public override string MappingDisplayName => $"{Config.targetField}";
        private FieldClearMapOptions Config { get { return (FieldClearMapOptions)_Config; } }

        internal override void InternalExecute(WorkItem source, WorkItem target)
        {
            if (!target.Fields.Contains(Config.targetField)) {
                Log.LogDebug("FieldClearMap: target field does not exist '{0}' for '{1}'", Config.targetField, target.Type.Name);
                return;
            }
            var targetField = target.Fields[Config.targetField];
            if (targetField.IsLimitedToAllowedValues && targetField.AllowedValues.Count > 0 && !targetField.AllowedValues.Contains(targetField.OriginalValue as string)) {
                Log.LogDebug("FieldClearMap: target field '{0}' is limited to allowed values list: '{1}'", Config.targetField, string.Join("', '", targetField.AllowedValues));
                return;
            }
            if (!targetField.IsEditable) {
                Log.LogDebug("FieldClearMap: target field '{0}' is not editable!", Config.targetField);
                return;
            }
            if (targetField.IsRequired) {
                Log.LogDebug("FieldClearMap: target field '{0}' is required!", Config.targetField);
                return;
            }
            targetField.Value = null;
            Log.LogDebug("FieldClearMap: field mapped {0}:{1} to {2} blanked", source.Id, target.Id, this.Config.targetField);
        }
    }
}