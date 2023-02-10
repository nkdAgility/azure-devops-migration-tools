using MigrationTools.Options;
using Newtonsoft.Json;

namespace MigrationTools._EngineV1.Configuration
{
    public interface IFieldMapConfig
    {
        string WorkItemTypeName { get; set; }

        [JsonIgnore]
        string FieldMap { get; }

        void SetExampleConfigDefaults();

    }
}