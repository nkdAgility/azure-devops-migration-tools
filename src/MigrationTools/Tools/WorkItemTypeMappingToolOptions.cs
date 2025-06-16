using System;
using System.Collections.Generic;
using MigrationTools.Enrichers;
using MigrationTools.Options;
using MigrationTools.Tools.Infrastructure;

namespace MigrationTools.Tools
{
    /// <summary>
    /// Configuration options for the WorkItemTypeMappingTool, defining how work item types should be mapped between source and target systems.
    /// </summary>
    public class WorkItemTypeMappingToolOptions : ToolOptions
    {

        /// <summary>
        /// List of work item mappings. 
        /// </summary>
        /// <default>{}</default>
        public Dictionary<string, string> Mappings { get; set; }

    }

    /// <summary>
    /// Represents a regular expression-based work item type mapping rule for dynamic type transformations.
    /// </summary>
    public class RegexWorkItemTypeMapping
    {
        /// <summary>
        /// Gets or sets a value indicating whether this mapping rule is enabled.
        /// </summary>
        public bool Enabled { get; set; }
        
        /// <summary>
        /// Gets or sets the regular expression pattern to match work item type names.
        /// </summary>
        public string Pattern { get; set; }
        
        /// <summary>
        /// Gets or sets the replacement string for matched work item type names.
        /// </summary>
        public string Replacement { get; set; }
        
        /// <summary>
        /// Gets or sets a description of what this mapping rule does.
        /// </summary>
        public string Description { get; set; }
    }
}