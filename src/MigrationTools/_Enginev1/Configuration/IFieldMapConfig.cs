using Newtonsoft.Json;

namespace MigrationTools.Configuration
{
    public interface IFieldMapConfig
    {
        string WorkItemTypeName { get; set; }

        [JsonIgnoreAttribute]
        string FieldMap { get; }
    }
}