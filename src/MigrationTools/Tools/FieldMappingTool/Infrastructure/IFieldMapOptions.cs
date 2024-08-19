using MigrationTools.Options;
using Newtonsoft.Json;

namespace MigrationTools.Tools.Infrastructure
{
    public interface IFieldMapOptions
    {
        string WorkItemTypeName { get; set; }

        [JsonIgnore]
        string FieldMap { get; }

        void SetExampleConfigDefaults();

    }
}