using System;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using MigrationTools.DataContracts.Pipelines;
using MigrationTools.Tools;
using MigrationTools.Tools.Infrastructure;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;
using static Microsoft.TeamFoundation.Client.CommandLine.Options;

namespace MigrationTools.FieldMaps.AzureDevops.ObjectModel;

#nullable enable

/// <summary>
/// Maps the value from a source field to a target field directly, with optional default value substitution for empty or null values.
/// </summary>
public class FieldToFieldMap : FieldMapBase
{
    public override string MappingDisplayName => $"{Config.sourceField} {Config.targetField}";
    private FieldToFieldMapOptions Config => (FieldToFieldMapOptions)_Config;

    /// <summary>
    /// Initializes a new instance of the FieldToFieldMap class.
    /// </summary>
    /// <param name="logger">Logger for the field map operations</param>
    /// <param name="telemetryLogger">Telemetry logger for tracking operations</param>
    public FieldToFieldMap(ILogger<FieldToFieldMap> logger, ITelemetryLogger telemetryLogger) : base(logger, telemetryLogger)
    {
    }

    /// <summary>
    /// Configures the field map with the specified options.
    /// </summary>
    /// <param name="config">The field map configuration options</param>
    public override void Configure(IFieldMapOptions config)
    {
        base.Configure(config);
    }

    internal override void InternalExecute(WorkItem source, WorkItem target)
    {
        if (!IsValid(source, target))
        {
            Log.LogWarning("FieldToFieldMap: [SKIPPED] Field mapping from '{SourceField}' to '{TargetField}' was skipped due to validation failures | Source WorkItem: {SourceId} -> Target WorkItem: {TargetId} | Mode: {FieldMapMode}",
                Config.sourceField, Config.targetField, source.Id, target.Id, Config.fieldMapMode);
            return;
        }

        string value;
        switch (Config.fieldMapMode)
        {
            case FieldMapMode.SourceToTarget:
                value = Convert.ToString(source.Fields[Config.sourceField]?.Value);
                break;
            case FieldMapMode.TargetToTarget:
                value = Convert.ToString(target.Fields[Config.sourceField]?.Value);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        if (string.IsNullOrEmpty(value) && Config.defaultValue is not null)
        {
            value = Config.defaultValue;
        }

        if (!string.IsNullOrEmpty(value))
        {
            target.Fields[Config.targetField].Value = value;
            Log.LogDebug("FieldToFieldMap: [UPDATE] Successfully mapped field {SourceField} to {TargetField} with value '{Value}' using mode {FieldMapMode} | Source WorkItem: {SourceId} -> Target WorkItem: {TargetId}",
               Config.sourceField, Config.targetField, value, Config.fieldMapMode, source.Id, target.Id);
        }
        else
        {
            Log.LogDebug("FieldToFieldMap: [SKIPPED] Proposed value is empty {SourceField} to {TargetField} with value '{Value}' using mode {FieldMapMode} | Source WorkItem: {SourceId} -> Target WorkItem: {TargetId}",
               Config.sourceField, Config.targetField, value, Config.fieldMapMode, source.Id, target.Id);
        }
    }

    private bool IsValid(WorkItem source, WorkItem target)
    {
        bool valid = true;
        switch (Config.fieldMapMode)
        {
            case FieldMapMode.SourceToTarget:
                if (!source.Fields.Contains(Config.sourceField))
                {
                    Log.LogWarning("FieldToFieldMap: [VALIDATION FAILED] Source field '{SourceField}' does not exist on source WorkItem {SourceId}. Please verify the field name is correct and exists in the source work item type. Available fields can be checked in Azure DevOps work item customization.", 
                        Config.sourceField, source.Id);
                    valid = false;
                }
                if (!target.Fields.Contains(Config.targetField))
                {
                    Log.LogWarning("FieldToFieldMap: [VALIDATION FAILED] Target field '{TargetField}' does not exist on target WorkItem {TargetId}. Please verify the field name is correct and exists in the target work item type. You may need to add this field to the target work item type or update your field mapping configuration.", 
                        Config.targetField, target.Id);
                    valid = false;
                }
                break;
            case FieldMapMode.TargetToTarget:
                if (!target.Fields.Contains(Config.sourceField))
                {
                    Log.LogWarning("FieldToFieldMap: [VALIDATION FAILED] Source field '{SourceField}' does not exist on target WorkItem {TargetId}. In TargetToTarget mode, both source and target fields must exist on the target work item. Please verify the source field name is correct.", 
                        Config.sourceField, target.Id);
                    valid = false;
                }
                if (!target.Fields.Contains(Config.targetField))
                {
                    Log.LogWarning("FieldToFieldMap: [VALIDATION FAILED] Target field '{TargetField}' does not exist on target WorkItem {TargetId}. In TargetToTarget mode, both source and target fields must exist on the target work item. Please verify the target field name is correct.", 
                        Config.targetField, target.Id);
                    valid = false;
                }
                break;
        }
        return valid;
    }
}
