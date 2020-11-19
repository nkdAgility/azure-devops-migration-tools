using System;
using System.Collections.Generic;

using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MigrationTools.Enrichers.Pipelines
{
    public partial class Pipelines
    {
        [JsonProperty("count")]
        public long Count { get; set; }

        [JsonProperty("value")]
        public Pipeline[] Value { get; set; }
    }
}
