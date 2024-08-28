using System;
using System.Collections.Generic;
using System.Text;
using MigrationTools.Options;
using MigrationTools.Tools.Infrastructure;
using Newtonsoft.Json;

namespace MigrationTools.Tools.Infrastructure
{
    public abstract class FieldMapOptions : IFieldMapOptions
    {
        [JsonIgnore]
        public string OptionFor => $"{GetType().Name.Replace("Options", "")}";

        [JsonIgnore]
        public ConfigurationMetadata ConfigurationMetadata => new ConfigurationMetadata
        {
            PathToInstance = $"MigrationTools:CommonTools:FieldMappingTool:FieldMaps:{OptionFor}",
            ObjectName = $"FieldMapType",
            OptionFor = OptionFor,
            PathToDefault = $"MigrationTools::CommonToolDefaults:FieldMappingTool:FieldMaps:{OptionFor}",
            PathToSample = $"MigrationTools::CommonToolSamples:FieldMappingTool:FieldMaps:{OptionFor}"
        };

        protected FieldMapOptions()
        {
            ApplyTo = new List<string>();
        }

        /// <summary>
        /// If set to `true` then the Fieldmap will run. Set to `false` and the processor will not run.
        /// </summary>
        [JsonIgnore]
        public bool Enabled { get; set; }
        public List<string> ApplyTo { get; set; }
    }
}
