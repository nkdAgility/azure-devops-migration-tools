using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using MigrationTools.Configuration.FieldMap;

namespace MigrationTools.Clients.AzureDevops.ObjectModel.FieldMaps
{
    public class FieldValueMap : FieldMapBase
    {
        public FieldValueMap(ILogger<FieldValueMap> logger) : base(logger)
        {
        }

        private FieldValueMapConfig Config { get { return (FieldValueMapConfig)_Config; } }

        public override string MappingDisplayName => $"{Config.sourceField} {Config.targetField}";

        internal override void InternalExecute(WorkItem source, WorkItem target)
        {
            if (source.Fields.Contains(Config.sourceField))
            {
                var sourceVal = source.Fields[Config.sourceField].Value;
                var t = target.Fields[Config.targetField].FieldDefinition.SystemType;

                if (sourceVal is null && Config.valueMapping.ContainsKey("null"))
                {
                    target.Fields[Config.targetField].Value = Convert.ChangeType(Config.valueMapping["null"], t);
                    Trace.WriteLine($"  [UPDATE] field value mapped {source.Id}:{Config.sourceField} to {target.Id}:{Config.targetField}");
                }
                else if (sourceVal != null && Config.valueMapping.ContainsKey(sourceVal.ToString()))
                {
                    target.Fields[Config.targetField].Value = Convert.ChangeType(Config.valueMapping[sourceVal.ToString()], t);
                    Trace.WriteLine($"  [UPDATE] field value mapped {source.Id}:{Config.sourceField} to {target.Id}:{Config.targetField}");
                }
                else if (sourceVal != null && !string.IsNullOrWhiteSpace(Config.defaultValue))
                {
                    target.Fields[Config.targetField].Value = Convert.ChangeType(Config.defaultValue, t);
                    Trace.WriteLine($"  [UPDATE] field set to default value {source.Id}:{Config.sourceField} to {target.Id}:{Config.targetField}");
                }
            }
        }
    }
}