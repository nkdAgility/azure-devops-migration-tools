using Newtonsoft.Json;

namespace MigrationTools.Processors.Pipelines
{
    public partial class ReleaseDefinitions
    {
        [JsonProperty("count")]
        public long Count { get; set; }

        [JsonProperty("value")]
        public ReleaseDefinition[] Value { get; set; }
    }
}