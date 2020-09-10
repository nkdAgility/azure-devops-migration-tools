using System;
using System.Collections.Generic;

namespace MigrationTools.Core.Configuration.Processing
{
    public class WorkItemQueryMigrationConfig : ITfsProcessingConfig
    {
        /// <summary>
        ///     The name of the shared folder, setting the default name
        /// </summary>
        private string sharedFolderName = "Shared Queries";

        /// <summary>
        ///     Do we add the source project name into the folder path
        /// </summary>
        public bool PrefixProjectToNodes { get; set; }

        /// <summary>
        ///     The name of the shared folder, made a parameter incase it every needs to be edited
        /// </summary>
        public string SharedFolderName
        {
            get { return sharedFolderName; }
            set { sharedFolderName = value; }
        }

        public Dictionary<string, string> SourceToTargetFieldMappings { get; set; }

        /// <inheritdoc />
        public bool Enabled { get; set; }

        /// <inheritdoc />
        public string Processor
        {
            get { return "WorkItemQueryMigrationContext"; }
        }

        /// <inheritdoc />
        public bool IsProcessorCompatible(IReadOnlyList<ITfsProcessingConfig> otherProcessors)
        {
            return true;
        }
    }
}