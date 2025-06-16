using System;
using System.Collections.Generic;
using MigrationTools.Processors.Infrastructure;

namespace MigrationTools.Processors
{
    /// <summary>
    /// Configuration options for the Process Definition Processor that migrates Azure DevOps process definitions and work item types between organizations.
    /// </summary>
    public class ProcessDefinitionProcessorOptions : ProcessorOptions
    {
        /// <summary>
        /// Dictionary mapping process names to lists of work item type names to be included in the migration. If null, all work item types will be migrated.
        /// </summary>
        public Dictionary<string, List<string>> Processes { get; set; }
        
        /// <summary>
        /// Dictionary mapping source process names to target process names for process template transformations during migration.
        /// </summary>
        public Dictionary<string, string> ProcessMaps { get; set; }

        /// <summary>
        /// Indicates whether to update existing process details in the target organization or only create new processes.
        /// </summary>
        public bool UpdateProcessDetails { get; set; }
        
        /// <summary>
        /// Maximum number of parallel operations to execute simultaneously during process definition migration to optimize performance.
        /// </summary>
        public int MaxDegreeOfParallelism { get; set; }

    }
}

