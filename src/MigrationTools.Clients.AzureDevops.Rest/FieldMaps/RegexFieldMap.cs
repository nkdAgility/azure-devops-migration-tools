using System;
using System.Text.RegularExpressions;
using System.Diagnostics;
using MigrationTools.Configuration.FieldMap;
using MigrationTools.Configuration;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;

namespace MigrationTools.Clients.AzureDevops.Rest.FieldMaps
{
    public class RegexFieldMap : FieldMapBase
    {
        private RegexFieldMapConfig Config { get { return (RegexFieldMapConfig)_Config; } }

        public override void Configure(IFieldMapConfig config)
        {
            base.Configure(config);
        }

        public override string MappingDisplayName => $"{Config.sourceField} {Config.targetField}";

        internal override void InternalExecute(WorkItem source, WorkItem target)
        {
            throw new NotImplementedException();
            //if (source.Fields.Contains(Config.sourceField) && source.Fields[Config.sourceField].Value != null && target.Fields.Contains(Config.targetField))
            //{
            //    if (Regex.IsMatch(source.Fields[Config.sourceField].Value.ToString(), Config.pattern))
            //    {
            //        target.Fields[Config.targetField].Value = Regex.Replace(source.Fields[Config.sourceField].Value.ToString(), Config.pattern, Config.replacement);
            //        Trace.WriteLine(string.Format("  [UPDATE] field tagged {0}:{1} to {2}:{3} with regex pattern of {4} resulting in {5}", source.Id, Config.sourceField, target.Id, Config.targetField, Config.pattern, target.Fields[Config.targetField].Value));
            //    }
            //}
        }
    }
}