using MigrationTools.Core.Configuration.FieldMap;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System;

namespace VstsSyncMigrator.Engine.ComponentContext
{
    public class FieldLiteralMap : FieldMapBase
    {
        private FieldLiteralMapConfig config;

        public FieldLiteralMap(FieldLiteralMapConfig config)
        {
            this.config = config;
            if (config.targetField == null)
            {
                throw new ArgumentNullException($"The target field `{config.targetField}` must be specified. Please use diferent fields.");
            }
        }

        public override string MappingDisplayName => $"{config.value} -> {config.targetField}";

        internal override void InternalExecute(WorkItem source, WorkItem target)
        {
            target.Fields[config.targetField].Value = config.value;
        }
    }
}
