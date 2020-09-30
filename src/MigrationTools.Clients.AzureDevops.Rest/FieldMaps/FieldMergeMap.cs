using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.ApplicationInsights;
using MigrationTools.Configuration.FieldMap;
using MigrationTools.Configuration;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;

namespace MigrationTools.Clients.AzureDevops.Rest.FieldMaps
{
    public class FieldMergeMap : FieldMapBase
    {
        private FieldMergeMapConfig Config { get { return (FieldMergeMapConfig)_Config; } }

        public override void Configure(IFieldMapConfig config)
        {
            base.Configure(config);
            if (Config.targetField == Config.sourceField2)
            {
                throw new ArgumentNullException($"The source field `{Config.sourceField2}` can not match target field `{Config.targetField}`. Please use diferent fields.");
            }
        }

        public override string MappingDisplayName => $"{Config.sourceField1}/{Config.sourceField2} -> {Config.targetField}";

        internal override void InternalExecute(WorkItem source, WorkItem target)
        {
            throw new NotImplementedException();
            //if (source.Fields.ContainsKey(Config.sourceField1) && source.Fields.ContainsKey(Config.sourceField2))
            //{
            //    var val1 = source.Fields[Config.sourceField1].Value != null ? source.Fields[Config.sourceField1].Value.ToString() : string.Empty;
            //    var val2 = source.Fields[Config.sourceField2].Value != null ? source.Fields[Config.sourceField2].Value.ToString() : string.Empty;
            //    var valT = target.Fields[Config.targetField].Value != null ? target.Fields[Config.targetField].Value.ToString() : string.Empty;
            //    var newValT = string.Format(Config.formatExpression, val1, val2);
            //    if (valT.Contains(val2) && val2.Trim().Length > 0)
            //    {
            //        Trace.WriteLine(string.Format("  [SKIP] field already merged {0}:{1}+{2} to {3}:{4}", source.Id, Config.sourceField1, Config.sourceField2, target.Id, Config.targetField));
            //    } else if (valT.Equals(newValT))
            //        {
            //        Trace.WriteLine(string.Format("  [SKIP] field already merged {0}:{1}+{2} to {3}:{4}", source.Id, Config.sourceField1, Config.sourceField2, target.Id, Config.targetField));
            //    } else
            //    {
            //        target.Fields[Config.targetField].Value = newValT;
            //        Trace.WriteLine(string.Format("  [UPDATE] field merged {0}:{1}+{2} to {3}:{4}", source.Id, Config.sourceField1, Config.sourceField2, target.Id, Config.targetField));
            //    }
            //}
        }
    }
}
