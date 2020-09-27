using MigrationTools.Core.Configuration.FieldMap;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System;
using MigrationTools.Core.Configuration;

namespace MigrationTools.Sinks.TfsObjectModel.FieldMaps
{
    public class FieldLiteralMap : FieldMapBase
    {
        private FieldLiteralMapConfig Config { get { return (FieldLiteralMapConfig)_Config; } }

        public override void Configure(IFieldMapConfig config)
        {
            base.Configure(config);

            if (Config.targetField == null)
            {
                throw new ArgumentNullException($"The target field `{Config.targetField}` must be specified. Please use diferent fields.");
            }
        }

        public override string MappingDisplayName => $"{Config.value} -> {Config.targetField}";

        internal override void InternalExecute(WorkItem source, WorkItem target)
        {
            target.Fields[Config.targetField].Value = Config.value;
        }
    }
}
