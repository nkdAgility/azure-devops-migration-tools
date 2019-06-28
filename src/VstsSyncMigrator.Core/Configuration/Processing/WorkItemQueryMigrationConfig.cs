using System;
using System.Collections.Generic;

namespace VstsSyncMigrator.Engine.Configuration.Processing
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

        /// <inheritdoc />
        public bool Enabled { get; set; }

        /// <inheritdoc />
        public Type Processor
        {
            get { return typeof(WorkItemQueryMigrationContext); }
        }

        /// <inheritdoc />
        public bool IsProcessorCompatible(IReadOnlyList<ITfsProcessingConfig> otherProcessors)
        {
            return true;
        }
    }
}