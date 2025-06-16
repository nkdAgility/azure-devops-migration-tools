using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using Microsoft.VisualStudio.Services.Commerce;
using MigrationTools._EngineV1.Configuration;
using MigrationTools.Tools;
using MigrationTools.Tools.Infrastructure;

namespace MigrationTools.FieldMaps.AzureDevops.ObjectModel
{
    /// <summary>
    /// Merges values from multiple source fields into a single target field using a specified format template.
    /// </summary>
    public class FieldMergeMap : FieldMapBase
    {
        /// <summary>
        /// Initializes a new instance of the FieldMergeMap class.
        /// </summary>
        /// <param name="logger">Logger for the field map operations</param>
        /// <param name="telemetryLogger">Telemetry logger for tracking operations</param>
        public FieldMergeMap(ILogger<FieldMergeMap> logger, ITelemetryLogger telemetryLogger) : base(logger, telemetryLogger)
        {
        }

        /// <summary>
        /// Gets the display name for this field mapping, showing the source fields and target field.
        /// </summary>
        public override string MappingDisplayName => buildMappingDisplayName();
        private FieldMergeMapOptions Config { get { return (FieldMergeMapOptions)_Config; } }

        /// <summary>
        /// Configures the field map with the specified options.
        /// </summary>
        /// <param name="config">The field map configuration options</param>
        public override void Configure(IFieldMapOptions config)
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
