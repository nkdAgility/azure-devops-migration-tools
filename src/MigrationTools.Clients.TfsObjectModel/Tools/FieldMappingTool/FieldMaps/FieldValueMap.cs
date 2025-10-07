﻿using System;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using MigrationTools.DataContracts;
using MigrationTools.Tools;

namespace MigrationTools.FieldMaps.AzureDevops.ObjectModel
{
    /// <summary>
    /// Maps field values based on a lookup table, allowing specific source values to be translated to different target values.
    /// </summary>
    public class FieldValueMap : FieldMapBase
    {
        /// <summary>
        /// Initializes a new instance of the FieldValueMap class.
        /// </summary>
        /// <param name="logger">Logger for the field map operations</param>
        /// <param name="telemetryLogger">Telemetry logger for tracking operations</param>
        public FieldValueMap(ILogger<FieldValueMap> logger, ITelemetryLogger telemetryLogger) : base(logger, telemetryLogger)
        {
        }

        internal FieldValueMapOptions Config { get { return (FieldValueMapOptions)_Config; } }

        public override string MappingDisplayName => $"{Config.sourceField} {Config.targetField}";

        internal override void InternalExecute(WorkItem source, WorkItem target)
        {
            throw new NotImplementedException("Use the WorkItemData overload instead");
        }

        internal override bool RefactoredToUseWorkItemData { get; } = true;

        internal override void InternalExecute(WorkItemData source, WorkItemData target)
        {
            if (source.Fields.ContainsKey(Config.sourceField))
            {
                var sourceVal = source.Fields[Config.sourceField];
                var targetWi = target.ToWorkItem();
                var t = targetWi.Fields[Config.targetField].FieldDefinition.SystemType;

                if (sourceVal is null && Config.valueMapping.ContainsKey("null"))
                {
                    targetWi.Fields[Config.targetField].Value = Convert.ChangeType(Config.valueMapping["null"], t);
                    Log.LogDebug("FieldValueMap: [UPDATE] field value mapped {SourceId}:{SourceField} to {TargetId}:{TargetField}", source.Id, Config.sourceField, target.Id, Config.targetField);
                }
                else if (sourceVal != null && sourceVal.Value == null)
                {
                    targetWi.Fields[Config.targetField].Value = Convert.ChangeType(Config.valueMapping["null"], t);
                    Log.LogDebug("FieldValueMap: [UPDATE] field value mapped {SourceId}:{SourceField} to {TargetId}:{TargetField}", source.Id, Config.sourceField, target.Id, Config.targetField);
                }
                else if (sourceVal != null && Config.valueMapping.ContainsKey(sourceVal.Value.ToString()))
                {
                    targetWi.Fields[Config.targetField].Value = Convert.ChangeType(Config.valueMapping[sourceVal.Value.ToString()], t);
                    Log.LogDebug("FieldValueMap: [UPDATE] field value mapped {SourceId}:{SourceField} to {TargetId}:{TargetField}", source.Id, Config.sourceField, target.Id, Config.targetField);
                }
                else if (sourceVal != null && !string.IsNullOrWhiteSpace(Config.defaultValue))
                {
                    targetWi.Fields[Config.targetField].Value = Convert.ChangeType(Config.defaultValue, t);
                    Log.LogDebug("FieldValueMap: [UPDATE] field value mapped {SourceId}:{SourceField} to {TargetId}:{TargetField}", source.Id, Config.sourceField, target.Id, Config.targetField);
                }
            }
        }
    }
}
