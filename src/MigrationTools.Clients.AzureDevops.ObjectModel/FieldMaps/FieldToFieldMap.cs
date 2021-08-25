using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using MigrationTools._EngineV1.Configuration;
using MigrationTools._EngineV1.Configuration.FieldMap;

namespace MigrationTools.FieldMaps.AzureDevops.ObjectModel
{
    public class FieldToFieldMap : FieldMapBase
    {
        public FieldToFieldMap(ILogger<FieldToFieldMap> logger) : base(logger)
        {
        }

        public override string MappingDisplayName => $"{Config.sourceField} {Config.targetField}";
        private FieldtoFieldMapConfig Config { get { return (FieldtoFieldMapConfig)_Config; } }

        public override void Configure(IFieldMapConfig config)
        {
            base.Configure(config);
        }

        internal override void InternalExecute(WorkItem source, WorkItem target)
        {
            if (source.Fields.Contains(Config.sourceField) && target.Fields.Contains(Config.targetField))
            {
                var value = source.Fields[Config.sourceField].Value;
                if (!(value is bool) && (value as string is null || value as string == "") && Config.defaultValue != null)
                {
                    value = Config.defaultValue;
                }
                target.Fields[Config.targetField].Value = value;
                Log.LogDebug("FieldToFieldMap: [UPDATE] field mapped {0}:{1} to {2}:{3}", source.Id, Config.sourceField, target.Id, Config.targetField);
            }
        }
    }
}