using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MigrationTools.Options
{
    /// <summary>
    /// Configuration options for defining work item queries used in migration processes. Supports WIQL (Work Item Query Language) queries with parameterization.
    /// </summary>
    public class QueryOptions
    {
        /// <summary>
        /// Gets or sets the WIQL (Work Item Query Language) query string used to select work items for processing. Must be a valid WIQL query.
        /// </summary>
        [Required]
        public string Query { get; set; }

        /// <summary>
        /// Gets or sets a dictionary of query parameters that can be substituted into the WIQL query. Key represents the parameter name, value represents the parameter value.
        /// </summary>
        public Dictionary<string, string> Parameters { get; set; }
    }
}