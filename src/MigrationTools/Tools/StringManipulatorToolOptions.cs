using System;
using System.Collections.Generic;
using MigrationTools.Enrichers;
using MigrationTools.Options;
using MigrationTools.Tools.Infrastructure;

namespace MigrationTools.Tools
{
    /// <summary>
    /// Configuration options for the StringManipulatorTool, defining string transformations and length limits for work item string fields.
    /// </summary>
    public class StringManipulatorToolOptions : ToolOptions
    {

        /// <summary>
        /// Max number of chars in a string. Applied last, and set to 1000000 by default.
        /// </summary>
        /// <default>1000000</default>
        public int MaxStringLength { get; set; }

        /// <summary>
        /// List of regex based string manipulations to apply to all string fields. Each regex replacement is applied in order and can be enabled or disabled.
        /// </summary>
        /// <default>{}</default>
        public List<RegexStringManipulator> Manipulators { get; set; }
    }

    /// <summary>
    /// Represents a regular expression-based string manipulation rule for processing work item string fields.
    /// </summary>
    public class RegexStringManipulator
    {
        /// <summary>
        /// Gets or sets a value indicating whether this manipulator is enabled.
        /// </summary>
        public bool Enabled { get; set; }
        
        /// <summary>
        /// Gets or sets the regular expression pattern to match.
        /// </summary>
        public string Pattern { get; set; }
        
        /// <summary>
        /// Gets or sets the replacement string for matched patterns.
        /// </summary>
        public string Replacement { get; set; }
        
        /// <summary>
        /// Gets or sets a description of what this manipulator does.
        /// </summary>
        public string Description { get; set; }
    }
}