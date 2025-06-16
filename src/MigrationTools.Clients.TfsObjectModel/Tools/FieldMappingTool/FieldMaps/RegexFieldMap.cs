using System;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using MigrationTools._EngineV1.Configuration;
using MigrationTools.Tools.Infrastructure;
using MigrationTools.DataContracts;
using MigrationTools.Tools;

namespace MigrationTools.FieldMaps.AzureDevops.ObjectModel
{
    /// <summary>
    /// Applies regular expression transformations to map values from a source field to a target field using pattern matching and replacement.
    /// </summary>
    public class RegexFieldMap : FieldMapBase
    {
        /// <summary>
        /// Initializes a new instance of the RegexFieldMap class.
        /// </summary>
        /// <param name="logger">Logger for the field map operations</param>
        /// <param name="telemetryLogger">Telemetry logger for tracking operations</param>
        public RegexFieldMap(ILogger<RegexFieldMap> logger, ITelemetryLogger telemetryLogger) : base(logger, telemetryLogger)
        {
        }

        public override string MappingDisplayName => $"{Config.sourceField} {Config.targetField}";
        private RegexFieldMapOptions Config { get { return (RegexFieldMapOptions)_Config; } }

        /// <summary>
        /// Configures the field map with the specified options.
        /// </summary>
        /// <param name="config">The field map configuration options</param>
        public override void Configure(IFieldMapOptions config)
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