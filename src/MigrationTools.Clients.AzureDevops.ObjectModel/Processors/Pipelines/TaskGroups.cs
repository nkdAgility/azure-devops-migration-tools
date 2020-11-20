using System;
using System.Collections.Generic;

using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MigrationTools.Enrichers.Pipelines
{
    public partial class TaskGroups
    {
        [JsonProperty("count")]
        public long Count { get; set; }

        [JsonProperty("value")]
        public TaskGroup[] Value { get; set; }
    }
}
