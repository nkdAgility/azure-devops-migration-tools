using System;
using System.Collections.Generic;
using System.Text;
using MigrationTools.Tools.Infrastructure;
using Newtonsoft.Json;

namespace MigrationTools.Tools.Infrastructure
{
    public abstract class FieldMapOptions : IFieldMapOptions
    {

        public virtual string ConfigurationSectionPath => $"MigrationTools:CommonTools:FieldMappingTool:FieldMapDefaults:{ConfigurationOptionFor}";

        public virtual string ConfigurationCollectionPath => $"MigrationTools:CommonTools:FieldMappingTool:FieldMaps:*:{ConfigurationOptionFor}";
        public virtual string ConfigurationCollectionObjectName => $"FieldMapType";

        public virtual string ConfigurationOptionFor => $"{GetType().Name.Replace("Options", "")}";
        /// <summary>
        /// If set to `true` then the Fieldmap will run. Set to `false` and the processor will not run.
        /// </summary>

        public bool Enabled { get; set; }
        public List<string> ApplyTo { get; set; }
    }
}
