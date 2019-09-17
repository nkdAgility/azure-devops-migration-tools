using System;
using System.Collections.Generic;
using VstsSyncMigrator.Engine.ComponentContext;

namespace VstsSyncMigrator.Engine.Configuration.FieldMap
{
    public class FieldtoFieldMultiMapConfig : IFieldMapConfig
    {
        public string WorkItemTypeName { get; set; }
        public Dictionary<string, string> SourceToTargetMappings { get; set; }
        public Type FieldMap
        {
            get
            {
                return typeof(FieldtoFieldMultiMap);
            }
        }
    }
}
