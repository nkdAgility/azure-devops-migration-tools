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
            IsCollection = true,
            PathToInstance = $"MigrationTools:CommonTools:FieldMappingTool:FieldMaps",
            ObjectName = $"FieldMapType",
            OptionFor = OptionFor,
            PathToDefault = $"MigrationTools:CommonTools:FieldMappingTool:FieldMapDefaults",
            PathToSample = $"MigrationTools:CommonToolSamples:FieldMappingTool:FieldMapSamples:{OptionFor}"
        };

        protected FieldMapOptions()
        {
            ApplyTo = new List<string>();
        }

        /// <summary>
        /// Not used: every configured field map runs for the work item types in `ApplyTo`. To turn a field map off, remove it from `FieldMaps`.
        /// </summary>
        [JsonIgnore]
        public bool Enabled { get; set; }
        public List<string> ApplyTo { get; set; }
    }
}
