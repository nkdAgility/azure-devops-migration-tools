using System.Collections.Generic;

namespace MigrationTools._EngineV1.Configuration
{
    public class EngineConfiguration
    {
        public string ChangeSetMappingFile { get; set; }
        public IMigrationClientConfig Source { get; set; }
        public IMigrationClientConfig Target { get; set; }

        public List<IFieldMapConfig> FieldMaps { get; set; }
        public Dictionary<string, string> GitRepoMapping { get; set; }

        public string LogLevel { get; set; }

        public List<IProcessorConfig> Processors { get; set; }
        public string Version { get; set; }
        public bool workaroundForQuerySOAPBugEnabled { get; set; }
        public Dictionary<string, string> WorkItemTypeDefinition { get; set; }
    }
}
