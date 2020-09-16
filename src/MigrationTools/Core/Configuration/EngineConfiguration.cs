using MigrationTools.Core.Configuration.FieldMap;
using MigrationTools.Core.Configuration.Processing;
using System;
using System.Collections.Generic;

namespace MigrationTools.Core.Configuration
{
    public class EngineConfiguration
    {
        public string Version { get; set; }
        public bool TelemetryEnableTrace { get; set; }
        public bool workaroundForQuerySOAPBugEnabled { get; set; }
        public string ChangeSetMappingFile { get; set; }
        public TeamProjectConfig Source { get; set; }
        public TeamProjectConfig Target { get; set; }
        public List<IFieldMapConfig> FieldMaps { get; set; }
        public Dictionary<string, string> WorkItemTypeDefinition { get; set; }
        public Dictionary<string, string> GitRepoMapping { get; set; }
        public List<ITfsProcessingConfig> Processors { get; set; }

    }
}
