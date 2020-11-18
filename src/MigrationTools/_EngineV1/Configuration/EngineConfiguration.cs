using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Serilog.Events;

namespace MigrationTools._EngineV1.Configuration
{
    public class EngineConfiguration
    {
        public virtual string ChangeSetMappingFile { get; set; }
        public virtual IMigrationClientConfig Source { get; set; }
        public virtual IMigrationClientConfig Target { get; set; }
        public virtual List<IFieldMapConfig> FieldMaps { get; set; }
        public virtual Dictionary<string, string> GitRepoMapping { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public virtual LogEventLevel LogLevel { get; set; }

        public virtual List<IProcessorConfig> Processors { get; set; }
        public virtual string Version { get; set; }
        public virtual bool workaroundForQuerySOAPBugEnabled { get; set; }
        public virtual Dictionary<string, string> WorkItemTypeDefinition { get; set; }
    }
}
