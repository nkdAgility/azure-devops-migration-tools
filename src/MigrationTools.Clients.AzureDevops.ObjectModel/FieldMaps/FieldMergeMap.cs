using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.VisualStudio.Services.Commerce;
using MigrationTools._EngineV1.Configuration;
using MigrationTools._EngineV1.Configuration.FieldMap;

namespace MigrationTools.FieldMaps.AzureDevops.ObjectModel
{
    public class FieldMergeMap : FieldMapBase
    {
        public FieldMergeMap(ILogger<FieldMergeMap> logger, ITelemetryLogger telemetryLogger) : base(logger, telemetryLogger)
        {
        }

        public override string MappingDisplayName => buildMappingDisplayName();
        private FieldMergeMapConfig Config { get { return (FieldMergeMapConfig)_Config; } }

        public override void Configure(IFieldMapConfig config)
        {
            base.Configure(config);
            // I think this is unessesary!
            //foreach (string item in Config.sourceFields)
            //{
            //    if (Config.targetField == item)
            //    {
            //        throw new ArgumentNullException($"The source field `{item}` can not match target field `{Config.targetField}`. Please use diferent fields.");
            //    }
            //}
            
        }

        internal string buildMappingDisplayName()
        {
            string result = "";
            foreach (string item in Config.sourceFields)
            {
                    result = result + item + $" /";
            }
            return result;
        }

        internal override void InternalExecute(WorkItem source, WorkItem target)
        {
            List<string> sourceData  = new List<string>();
            foreach (string item in Config.sourceFields)
            {
                if (source.Fields[item].Value != null)
                {
                    sourceData.Add(source.Fields[item].Value.ToString());
                }
                else
                {
                    sourceData.Add("");
                }
            }
            var newValT = string.Format(Config.formatExpression, sourceData.ToArray());
            target.Fields[Config.targetField].Value = newValT;
        }
    }
}
