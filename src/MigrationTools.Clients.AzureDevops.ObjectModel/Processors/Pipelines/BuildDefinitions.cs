using Newtonsoft.Json;

namespace MigrationTools.Processors.Pipelines
{
    public partial class BuildDefinitions
    {
        [JsonProperty("count")]
        public long Count { get; set; }

        [JsonProperty("value")]
        public BuildDefinition[] Value { get; set; }
    }
}