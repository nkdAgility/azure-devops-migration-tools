using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using MigrationTools._EngineV1.Configuration;
using MigrationTools.Tools.Infrastructure;
using MigrationTools.Tools;

namespace MigrationTools.FieldMaps.AzureDevops.ObjectModel
{
    /// <summary>
    /// Maps work item area path or iteration path hierarchies to tags, allowing tree structures to be represented as flat tag collections.
    /// </summary>
    public class TreeToTagFieldMap : FieldMapBase
    {
        /// <summary>
        /// Initializes a new instance of the TreeToTagFieldMap class.
        /// </summary>
        /// <param name="logger">Logger for the field map operations</param>
        /// <param name="telemetryLogger">Telemetry logger for tracking operations</param>
        public TreeToTagFieldMap(ILogger<TreeToTagFieldMap> logger, ITelemetryLogger telemetryLogger) : base(logger, telemetryLogger)
        {
        }

        private TreeToTagFieldMapOptions Config { get { return (TreeToTagFieldMapOptions)_Config; } }

        /// <summary>
        /// Configures the field map with the specified options.
        /// </summary>
        /// <param name="config">The field map configuration options</param>
        public override void Configure(IFieldMapOptions config)
        {
            base.Configure(config);
        }

        public override string MappingDisplayName => string.Empty;

        internal override void InternalExecute(WorkItem source, WorkItem target)
        {
            List<string> newTags = target.Tags.Split(char.Parse(@";")).ToList();

            string value;

            if (Config.timeTravel > 0)
            {
                value = (string)source.Revisions[source.Revision - Config.timeTravel].Fields["System.AreaPath"].Value;
            }
            else
            {
                value = source.AreaPath;
            }

            List<string> bits = new List<string>(value.Split(char.Parse(@"\"))).Skip(Config.toSkip).ToList();
            target.Tags = string.Join(";", newTags.Union(bits).ToArray());
        }
    }
}