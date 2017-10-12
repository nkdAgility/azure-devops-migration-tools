//New COmment
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        internal override void InternalExecute(WorkItem source, WorkItem target)
        {
            if (target.Fields.Contains(config.targetField))
            { 
                target.Fields[config.targetField].Value = "";
                Trace.WriteLine(string.Format("  [UPDATE] field mapped {0}:{1} to {2} blanked", source.Id, target.Id, this.config.targetField));
            }
        }
    }
}
