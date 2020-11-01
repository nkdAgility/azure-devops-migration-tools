using System;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using MigrationTools.Configuration;
using MigrationTools.Configuration.FieldMap;

namespace MigrationTools.FieldMaps.AzureDevops.ObjectModel
{
    public class FieldMergeMap : FieldMapBase
    {
        public FieldMergeMap(ILogger<FieldMergeMap> logger) : base(logger)
        {
        }

        public override string MappingDisplayName => $"{Config.sourceField1}/{Config.sourceField2} -> {Config.targetField}";
        private FieldMergeMapConfig Config { get { return (FieldMergeMapConfig)_Config; } }

        public override void Configure(IFieldMapConfig config)
        {
            base.Configure(config);
            if (Config.targetField == Config.sourceField2)
            {
                throw new ArgumentNullException($"The source field `{Config.sourceField2}` can not match target field `{Config.targetField}`. Please use diferent fields.");
            }
        }

        internal override void InternalExecute(WorkItem source, WorkItem target)
        {
            if (source.Fields.Contains(Config.sourceField1) && source.Fields.Contains(Config.sourceField2))
            {
                var val1 = source.Fields[Config.sourceField1].Value != null ? source.Fields[Config.sourceField1].Value.ToString() : string.Empty;
                var val2 = source.Fields[Config.sourceField2].Value != null ? source.Fields[Config.sourceField2].Value.ToString() : string.Empty;
                var valT = target.Fields[Config.targetField].Value != null ? target.Fields[Config.targetField].Value.ToString() : string.Empty;
                var newValT = string.Format(Config.formatExpression, val1, val2);
                if (valT.Contains(val2) && val2.Trim().Length > 0)
                {
                    Log.LogDebug("FieldMergeMap: [SKIP] field already merged {0}:{1}+{2} to {3}:{4}", source.Id, Config.sourceField1, Config.sourceField2, target.Id, Config.targetField);
                }
                else if (valT.Equals(newValT))
                {
                    Log.LogDebug("FieldMergeMap: [SKIP] field already merged {0}:{1}+{2} to {3}:{4}", source.Id, Config.sourceField1, Config.sourceField2, target.Id, Config.targetField);
                }
                else
                {
                    target.Fields[Config.targetField].Value = newValT;
                    Log.LogDebug("FieldMergeMap: [UPDATE] field merged {0}:{1}+{2} to {3}:{4}", source.Id, Config.sourceField1, Config.sourceField2, target.Id, Config.targetField);
                }
            }
        }
    }
}