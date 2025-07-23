using System.Collections.Generic;
using MigrationTools.Tools.Infrastructure;

namespace MigrationTools.Tools
{
    /// <summary>
    /// Configuration options for the TFS Validate Required Field Tool that ensures all required fields are properly populated during work item migration.
    /// </summary>
    public class TfsValidateRequiredFieldToolOptions : ToolOptions
    {
        /// <summary>
        /// Add a list of work item types from the source that you want to exclude from validation. This is a case-insensitive comparison.
        /// WARNING: If you exclude a work item type that exists in the migration dataset, the migration will fail when trying to.
        /// </summary>
        /// <default>[]</default>
        public List<string> Exclusions { get; set; } = new List<string>();
    }
}
