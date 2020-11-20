using Newtonsoft.Json;

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