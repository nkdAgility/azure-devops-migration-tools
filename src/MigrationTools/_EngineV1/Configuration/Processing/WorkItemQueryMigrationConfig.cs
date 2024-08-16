using System.Collections.Generic;
using MigrationTools.Enrichers;

namespace MigrationTools._EngineV1.Configuration.Processing
{
    public class WorkItemQueryMigrationConfig : IProcessorConfig
    {
        /// <summary>
        /// The name of the shared folder, setting the default name
        /// </summary>
        /// <default>Shared Queries</default>
        private string sharedFolderName = "Shared Queries";

        /// <summary>
        /// Prefix your iterations and areas with the project name. If you have enabled this in `NodeStructuresMigrationConfig` you must do it here too.
        /// </summary>
        /// <default>false</default>
        public bool PrefixProjectToNodes { get; set; }

        /// <summary>
        /// The name of the shared folder, made a parameter incase it every needs to be edited
        /// </summary>
        /// <default>none</default>
        public string SharedFolderName
        {
            get { return sharedFolderName; }
            set { sharedFolderName = value; }
        }

        /// <summary>
        /// Any field mappings
        /// </summary>
        /// <default>none</default>
        public Dictionary<string, string> SourceToTargetFieldMappings { get; set; }

        /// <inheritdoc />
        public bool Enabled { get; set; }

        /// <summary>
        /// A list of enrichers that can augment the proccessing of the data
        /// </summary>
        public List<IProcessorEnricher> Enrichers { get; set; }

        /// <inheritdoc />
        public string Processor
        {
            get { return "WorkItemQueryMigrationContext"; }
        }

        /// <inheritdoc />
        public bool IsProcessorCompatible(IReadOnlyList<IProcessorConfig> otherProcessors)
        {
            return true;
        }
    }
}