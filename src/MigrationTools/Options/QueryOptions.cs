using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MigrationTools.Options
{
    public class QueryOptions
    {
        [Required]
        public string Query { get; set; }

        public Dictionary<string, string> Parameters { get; set; }
    }
}