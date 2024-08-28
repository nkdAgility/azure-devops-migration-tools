using System;
using System.Collections.Generic;
using System.Text;
using MigrationTools.Options;
using Newtonsoft.Json;

namespace MigrationTools.Tools.Infrastructure
{
    public abstract class ToolOptions : IToolOptions
    {
        [JsonIgnore]
        public string OptionFor => $"{GetType().Name.Replace("Options", "")}";

        [JsonIgnore]
        public ConfigurationMetadata ConfigurationMetadata => new ConfigurationMetadata
        {
            PathToInstance = $"MigrationTools:CommonTools:{OptionFor}",
            ObjectName = $"ToolType",
            OptionFor = OptionFor,
            PathToDefault = $"MigrationTools:CommonTools:{OptionFor}",
            PathToSample = $"MigrationTools:CommonToolSamples:{OptionFor}"
        };

        /// <summary>
        /// If set to `true` then the tool will run. Set to `false` and the processor will not run.
        /// </summary>

        public bool Enabled { get; set; }

    }
}
