using System.Collections.Generic;

namespace MigrationTools.Options
{
    public class QueryOptions
    {
        public string Query { get; set; }

        public Dictionary<string, object> Paramiters { get; set; }
    }
}