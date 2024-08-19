using MigrationTools.Options;
using Newtonsoft.Json;

namespace MigrationTools._EngineV1.Configuration
{
    public interface IFieldMapOptions
    {
        string WorkItemTypeName { get; set; }

        [JsonIgnore]
        string FieldMap { get; }

        void SetExampleConfigDefaults();

    }
}