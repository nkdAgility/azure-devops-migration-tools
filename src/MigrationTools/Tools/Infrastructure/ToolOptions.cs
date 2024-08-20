using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace MigrationTools.Tools.Infrastructure
{
    public abstract class ToolOptions : IToolOptions
    {
        [JsonIgnore]
        public virtual string ConfigurationSectionPath => $"MigrationTools:CommonTools:{ConfigurationOptionFor}";
        [JsonIgnore]
        public string ConfigurationCollectionPath => null;
        [JsonIgnore]
        public string ConfigurationObjectName => $"ToolType";
        [JsonIgnore]
        public virtual string ConfigurationOptionFor => $"{GetType().Name.Replace("Options", "")}";

        /// <summary>
        /// If set to `true` then the processor will run. Set to `false` and the processor will not run.
        /// </summary>

        public bool Enabled { get; set; }

    }
}
