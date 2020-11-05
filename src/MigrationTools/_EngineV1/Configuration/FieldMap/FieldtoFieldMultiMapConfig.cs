using System.Collections.Generic;

namespace MigrationTools._EngineV1.Configuration.FieldMap
{
    public class FieldtoFieldMultiMapConfig : IFieldMapConfig
    {
        public string WorkItemTypeName { get; set; }
        public Dictionary<string, string> SourceToTargetMappings { get; set; }

        public string FieldMap
        {
            get
            {
                return "FieldtoFieldMultiMap";
            }
        }
    }
}