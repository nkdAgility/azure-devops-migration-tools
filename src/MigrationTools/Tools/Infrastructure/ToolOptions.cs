using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace MigrationTools.Tools.Infrastructure
{
    public abstract class ToolOptions : IToolOptions
    {
        [JsonIgnore]
        public string ConfigurationSectionPath => $"MigrationTools:CommonTools:{ConfigurationOptionFor}";

        [JsonIgnore]
        public string ConfigurationCollectionPath => null;
        [JsonIgnore]
        public string ConfigurationObjectName => $"ToolType";
        [JsonIgnore]
        public string ConfigurationSamplePath => $"MigrationTools:CommonToolsSamples:{ConfigurationOptionFor}";
        [JsonIgnore]
        public string ConfigurationOptionFor => $"{GetType().Name.Replace("Options", "")}";

        /// <summary>
        /// If set to `true` then the tool will run. Set to `false` and the processor will not run.
        /// </summary>

        public bool Enabled { get; set; }

    }
}
