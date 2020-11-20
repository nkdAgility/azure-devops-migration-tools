using Newtonsoft.Json;

namespace MigrationTools.Enrichers.Pipelines
{
    public partial class BuildDefinitions
    {
        [JsonProperty("count")]
        public long Count { get; set; }

        [JsonProperty("value")]
        public BuildDefinition[] Value { get; set; }
    }
}