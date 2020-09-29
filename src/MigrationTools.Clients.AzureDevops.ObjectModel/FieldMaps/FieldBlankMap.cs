//New COmment
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System.Diagnostics;
using MigrationTools.Core.Configuration.FieldMap;

namespace MigrationTools.Clients.AzureDevops.ObjectModel.FieldMaps
{
    public class FieldBlankMap : FieldMapBase
    {
        private FieldBlankMapConfig Config { get { return (FieldBlankMapConfig)_Config; } }


        public override string MappingDisplayName => $"{Config.targetField}";

        internal override void InternalExecute(WorkItem source, WorkItem target)
        {
            if (target.Fields.Contains(Config.targetField))
            { 
                target.Fields[Config.targetField].Value = "";
                Trace.WriteLine(string.Format("  [UPDATE] field mapped {0}:{1} to {2} blanked", source.Id, target.Id, this.Config.targetField));
            }
        }
    }
}
