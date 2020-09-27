//New COmment
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using MigrationTools.Core.Configuration.FieldMap;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;

namespace MigrationTools.Sinks.AzureDevOps.FieldMaps
{
    public class FieldBlankMap : FieldMapBase
    {
        private FieldBlankMapConfig Config { get { return (FieldBlankMapConfig)_Config; } }


        public override string MappingDisplayName => $"{Config.targetField}";

        internal override void InternalExecute(WorkItem source, WorkItem target)
        {
            throw new NotImplementedException();
            //if (target.Fields.ContainsKey(Config.targetField))
            //{ 
            //    (target.Fields[Config.targetField]).Value = "";
            //    Trace.WriteLine(string.Format("  [UPDATE] field mapped {0}:{1} to {2} blanked", source.Id, target.Id, this.Config.targetField));
            //}
        }
    }
}
