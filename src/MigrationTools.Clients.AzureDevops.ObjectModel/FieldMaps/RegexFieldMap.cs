using System;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using MigrationTools._EngineV1.Configuration;
using MigrationTools._EngineV1.Configuration.FieldMap;
using MigrationTools.DataContracts;

namespace MigrationTools.FieldMaps.AzureDevops.ObjectModel
{
    public class RegexFieldMap : FieldMapBase
    {
        public RegexFieldMap(ILogger<RegexFieldMap> logger, ITelemetryLogger telemetryLogger) : base(logger, telemetryLogger)
        {
        }

        public override string MappingDisplayName => $"{Config.sourceField} {Config.targetField}";
        private RegexFieldMapOptions Config { get { return (RegexFieldMapOptions)_Config; } }

        public override void Configure(IFieldMapConfig config)
        {
            base.Configure(config);
        }

        internal override bool RefactoredToUseWorkItemData { get; } = true;

        internal override void InternalExecute(WorkItem source, WorkItem target)
        {
            throw new NotImplementedException("Use the WorkItemData overload instead");
        }

        internal override void InternalExecute(WorkItemData source, WorkItemData target)
        {
            var targetWi = target.ToWorkItem();
            if (source.Fields.TryGetValue(Config.sourceField, out var sourceField) && sourceField.Value != null && targetWi.Fields.Contains(Config.targetField))
            {
                var inputData = sourceField.Value.ToString();
                if (Regex.IsMatch(inputData, Config.pattern))
                {
                    var transformedData = Regex.Replace(inputData, Config.pattern, Config.replacement);
                    targetWi.Fields[Config.targetField].Value = transformedData;
                    target.Fields[Config.targetField].Value = transformedData;
                    Log.LogDebug("FieldValuetoTagMap: [UPDATE] field tagged {0}:{1} to {2}:{3} with regex pattern of {4} resulting in {5}", source.Id, Config.sourceField, target.Id, Config.targetField, Config.pattern, target.Fields[Config.targetField].Value);
                }
            }
        }
    }
}