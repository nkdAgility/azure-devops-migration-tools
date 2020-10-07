using System.Text.Json.Serialization;

namespace MigrationTools.Configuration
{
    public interface IMigrationClientConfig
    {
        [JsonIgnoreAttribute]
        System.Type MigrationClient { get; }

        MigrationClientClientDirection Direction { get; }
    }
}