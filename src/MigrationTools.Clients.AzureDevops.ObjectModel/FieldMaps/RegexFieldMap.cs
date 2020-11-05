using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using MigrationTools._EngineV1.Configuration;
using MigrationTools._EngineV1.Configuration.FieldMap;

namespace MigrationTools.FieldMaps.AzureDevops.ObjectModel
{
    public class RegexFieldMap : FieldMapBase
    {
        public RegexFieldMap(ILogger<RegexFieldMap> logger) : base(logger)
        {
        }

        public override string MappingDisplayName => $"{Config.sourceField} {Config.targetField}";
        private RegexFieldMapConfig Config { get { return (RegexFieldMapConfig)_Config; } }

        public override void Configure(IFieldMapConfig config)
        {
            base.Configure(config);
        }

        internal override void InternalExecute(WorkItem source, WorkItem target)
        {
            if (source.Fields.Contains(Config.sourceField) && source.Fields[Config.sourceField].Value != null && target.Fields.Contains(Config.targetField))
            {
                if (Regex.IsMatch(source.Fields[Config.sourceField].Value.ToString(), Config.pattern))
                {
                    target.Fields[Config.targetField].Value = Regex.Replace(source.Fields[Config.sourceField].Value.ToString(), Config.pattern, Config.replacement);
                    Log.LogDebug("FieldValuetoTagMap: [UPDATE] field tagged {0}:{1} to {2}:{3} with regex pattern of {4} resulting in {5}", source.Id, Config.sourceField, target.Id, Config.targetField, Config.pattern, target.Fields[Config.targetField].Value);
                }
            }
        }
    }
}