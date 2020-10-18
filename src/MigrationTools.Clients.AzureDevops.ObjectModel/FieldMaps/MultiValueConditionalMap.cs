using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using MigrationTools.Configuration;
using MigrationTools.Configuration.FieldMap;

namespace MigrationTools.FieldMaps.AzureDevops.ObjectModel
{
    public class MultiValueConditionalMap : FieldMapBase
    {
        public MultiValueConditionalMap(ILogger<MultiValueConditionalMap> logger) : base(logger)
        {
        }

        public override string MappingDisplayName => string.Empty;
        private MultiValueConditionalMapConfig Config { get { return (MultiValueConditionalMapConfig)_Config; } }

        public override void Configure(IFieldMapConfig config)
        {
            base.Configure(config);
        }

        internal override void InternalExecute(WorkItem source, WorkItem target)
        {
            if (fieldsExist(Config.sourceFieldsAndValues, source) && fieldsExist(Config.targetFieldsAndValues, target))
            {
                if (fieldsValueMatch(Config.sourceFieldsAndValues, source))
                {
                    fieldsUpdate(Config.targetFieldsAndValues, target);
                }
                Log.LogDebug("FieldValuetoTagMap: [UPDATE] field mapped {0}:{1} to {2}:{3}", source.Id, Config.sourceFieldsAndValues.Keys.ToString(), target.Id, Config.targetFieldsAndValues.Keys.ToString());
            }
            else
            {
                Log.LogDebug("FieldValuetoTagMap: [SKIPPED] Not all source and target fields exist", source.Id, Config.sourceFieldsAndValues.Keys.ToString(), target.Id, Config.targetFieldsAndValues.Keys.ToString());
            }
        }

        private bool fieldsExist(Dictionary<string, string> fieldsAndValues, WorkItem workitem)
        {
            bool exists = true;
            foreach (string field in fieldsAndValues.Keys)
            {
                if (!workitem.Fields.Contains(field))
                {
                    exists = false;
                }
            }
            return exists;
        }

        private void fieldsUpdate(Dictionary<string, string> fieldAndValues, WorkItem workitem)
        {
            foreach (string field in fieldAndValues.Keys)
            {
                workitem.Fields[field].Value = fieldAndValues[field];
            }
        }

        private bool fieldsValueMatch(Dictionary<string, string> fieldAndValues, WorkItem workitem)
        {
            bool matches = true;
            foreach (string field in fieldAndValues.Keys)
            {
                if ((string)workitem.Fields[field].Value != fieldAndValues[field])
                {
                    matches = false;
                }
            }
            return matches;
        }
    }
}