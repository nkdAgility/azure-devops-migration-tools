using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System.Diagnostics;
using VstsSyncMigrator.Engine.Configuration.FieldMap;

namespace VstsSyncMigrator.Engine.ComponentContext
{
    public class FieldBlankMap : FieldMapBase
    {
        private FieldBlankMapConfig config;

        public FieldBlankMap(FieldBlankMapConfig config)
        {
            this.config = config;
        }

        public override string MappingDisplayName => $"{config.targetField}";

        internal override void InternalExecute(WorkItem source, WorkItem target)
        {
            if (target.Fields.Contains(config.targetField))
            {
                target.Fields[config.targetField].Value = null;
                Trace.WriteLine(string.Format("  [UPDATE] field mapped {0}:{1} to {2} blanked", source.Id, target.Id, this.config.targetField));
            }
        }
    }
}
