using Newtonsoft.Json;

namespace MigrationTools.Enrichers.Pipelines
{
    public partial class ReleaseDefinitions
    {
        [JsonProperty("count")]
        public long Count { get; set; }

        [JsonProperty("value")]
        public ReleaseDefinition[] Value { get; set; }
    }
}