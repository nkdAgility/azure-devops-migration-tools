//New COmment
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using MigrationTools._EngineV1.Configuration.FieldMap;

namespace MigrationTools.FieldMaps.AzureDevops.ObjectModel
{
    public class FieldBlankMap : FieldMapBase
    {
        public FieldBlankMap(ILogger<FieldBlankMap> logger) : base(logger)
        {
        }

        public override string MappingDisplayName => $"{Config.targetField}";
        private FieldBlankMapConfig Config { get { return (FieldBlankMapConfig)_Config; } }

        internal override void InternalExecute(WorkItem source, WorkItem target)
        {
            if (!target.Fields.Contains(Config.targetField)) {
                Log.LogDebug($"FieldBlankMap: target field does not exist '{Config.targetField}' for '{target.Type.Name}'");
                return;
            }
            var targetField = target.Fields[Config.targetField];
            if (targetField.IsLimitedToAllowedValues && targetField.AllowedValues.Count > 0 && !targetField.AllowedValues.Contains(targetField.OriginalValue as string)) {
                Log.LogDebug($"FieldBlankMap: target field '{Config.targetField}' is limited to allowed values list: '{string.Join("', '", targetField.AllowedValues)}'");
                return;
            }
            if (!targetField.IsEditable) {
                Log.LogDebug($"FieldBlankMap: target field '{Config.targetField}' is not editable!");
                return;
            }
            if (targetField.IsRequired) {
                Log.LogDebug($"FieldBlankMap: target field '{Config.targetField}' is required!");
                return;
            }
            targetField.Value = targetField.OriginalValue;
            Log.LogDebug("FieldBlankMap: field mapped {0}:{1} to {2} blanked", source.Id, target.Id, this.Config.targetField);
        }
    }
}