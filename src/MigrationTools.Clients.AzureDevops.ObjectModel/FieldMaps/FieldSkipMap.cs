//New COmment
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using MigrationTools._EngineV1.Configuration.FieldMap;

namespace MigrationTools.FieldMaps.AzureDevops.ObjectModel
{
    public class FieldSkipMap : FieldMapBase
    {
        public FieldSkipMap(ILogger<FieldSkipMap> logger) : base(logger)
        {
        }

        public override string MappingDisplayName => $"{Config.targetField}";
        private FieldSkipMapConfig Config { get { return (FieldSkipMapConfig)_Config; } }

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