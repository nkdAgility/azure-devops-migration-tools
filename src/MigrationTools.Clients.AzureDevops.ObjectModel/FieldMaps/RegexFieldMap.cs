using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using MigrationTools.Configuration;
using MigrationTools.Configuration.FieldMap;

namespace MigrationTools.Clients.AzureDevops.ObjectModel.FieldMaps
{
    public class RegexFieldMap : FieldMapBase
    {

        public RegexFieldMap(ILogger<RegexFieldMap> logger) : base(logger)
        {

        }
        private RegexFieldMapConfig Config { get { return (RegexFieldMapConfig)_Config; } }

        public override void Configure(IFieldMapConfig config)
        {
            base.Configure(config);
        }

        public override string MappingDisplayName => $"{Config.sourceField} {Config.targetField}";

        internal override void InternalExecute(WorkItem source, WorkItem target)
        {
            if (source.Fields.Contains(Config.sourceField) && source.Fields[Config.sourceField].Value != null && target.Fields.Contains(Config.targetField))
            {
                if (Regex.IsMatch(source.Fields[Config.sourceField].Value.ToString(), Config.pattern))
                {
                    target.Fields[Config.targetField].Value = Regex.Replace(source.Fields[Config.sourceField].Value.ToString(), Config.pattern, Config.replacement);
                    Trace.WriteLine(string.Format("  [UPDATE] field tagged {0}:{1} to {2}:{3} with regex pattern of {4} resulting in {5}", source.Id, Config.sourceField, target.Id, Config.targetField, Config.pattern, target.Fields[Config.targetField].Value));
                }
            }
        }
    }
}