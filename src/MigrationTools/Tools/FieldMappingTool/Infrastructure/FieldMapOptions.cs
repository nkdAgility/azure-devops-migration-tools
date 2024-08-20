using System;
using System.Collections.Generic;
using System.Text;
using MigrationTools.Tools.Infrastructure;
using Newtonsoft.Json;

namespace MigrationTools.Tools.Infrastructure
{
    public abstract class FieldMapOptions : IFieldMapOptions
    {
        protected FieldMapOptions()
        {
            ApplyTo = new List<string>();
        }

        public string ConfigurationSectionPath => $"MigrationTools:CommonTools:FieldMappingTool:FieldMapDefaults:{ConfigurationOptionFor}";
        public string ConfigurationCollectionPath => $"MigrationTools:CommonTools:FieldMappingTool:FieldMaps";
        public string ConfigurationObjectName => $"FieldMapType";

        public string ConfigurationOptionFor => $"{GetType().Name.Replace("Options", "")}";
        /// <summary>
        /// If set to `true` then the Fieldmap will run. Set to `false` and the processor will not run.
        /// </summary>
        [JsonIgnore]
        public bool Enabled { get; set; }
        public List<string> ApplyTo { get; set; }
    }
}
