//New COmment
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using MigrationTools.Configuration.FieldMap;

namespace MigrationTools.Clients.AzureDevops.ObjectModel.FieldMaps
{
    public class FieldBlankMap : FieldMapBase
    {
        public FieldBlankMap(ILogger<FieldBlankMap> logger) : base(logger)
        {
        }

        public override string MappingDisplayName => $"{Config.targetField}";
        private FieldBlankMapConfig Config { get { return (FieldBlankMapConfig)_Config; } }

        internal override void InternalExecute(WorkItem source, WorkItem target)
        {
            if (target.Fields.Contains(Config.targetField))
            {
                target.Fields[Config.targetField].Value = "";
                Log.LogDebug("FieldBlankMap: field mapped {0}:{1} to {2} blanked", source.Id, target.Id, this.Config.targetField);
            }
        }
    }
}