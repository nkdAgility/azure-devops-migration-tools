using System;
using System.Collections.Generic;
using MigrationTools.Endpoints;

namespace MigrationTools.Processors
{
    /// <summary>
    /// The `TfsSharedQueryProcessor` enabled you to migrate queries from one location to another.
    /// </summary>
    public class TfsSharedQueryProcessorOptions : ProcessorOptions
    {
        /// <summary>
        /// Do we add the source project name into the folder path
        /// </summary>
        /// <default>false</default>
        public bool PrefixProjectToNodes { get; set; }

        /// <summary>
        /// The name of the shared folder, made a parameter incase it every needs to be edited
        /// </summary>
        /// <default>Shared Queries</default>
        public string SharedFolderName { get; set; } = "Shared Queries";

        /// <summary>
        /// Mapping of the source to the target
        /// </summary>
        public Dictionary<string, string> SourceToTargetFieldMappings { get; set; }

    }
}