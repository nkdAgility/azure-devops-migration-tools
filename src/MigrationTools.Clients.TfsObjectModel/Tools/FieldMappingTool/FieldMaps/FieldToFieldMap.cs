using System;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using MigrationTools.Tools;
using MigrationTools.Tools.Infrastructure;

namespace MigrationTools.FieldMaps.AzureDevops.ObjectModel;

#nullable enable

public class FieldToFieldMap : FieldMapBase
{
    public override string MappingDisplayName => $"{Config.sourceField} {Config.targetField}";
    private FieldToFieldMapOptions Config => (FieldToFieldMapOptions)_Config;

    public FieldToFieldMap(ILogger<FieldToFieldMap> logger, ITelemetryLogger telemetryLogger) : base(logger, telemetryLogger)
    {
    }

    public override void Configure(IFieldMapOptions config)
    {
        base.Configure(config);
    }

    internal override void InternalExecute(WorkItem source, WorkItem target)
    {
        if (!source.Fields.Contains(Config.sourceField) || !target.Fields.Contains(Config.targetField))
        {
            return;
        }

        var value = Convert.ToString(source.Fields[Config.sourceField]?.Value);
        if (string.IsNullOrEmpty(value) && Config.defaultValue is not null)
        {
            value = Config.defaultValue;
        }

        target.Fields[Config.targetField].Value = value;
        Log.LogDebug("FieldToFieldMap: [UPDATE] field mapped {0}:{1} to {2}:{3}", source.Id, Config.sourceField, target.Id, Config.targetField);
    }
}