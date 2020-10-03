using System.Collections.Generic;

namespace MigrationTools.Configuration.FieldMap
{
    public class FieldValueMapConfig : IFieldMapConfig
    {
        public string WorkItemTypeName { get; set; }
        public string sourceField { get; set; }
        public string targetField { get; set; }
        public string defaultValue { get; set; }
        public Dictionary<string, string> valueMapping { get; set; }


        public string FieldMap
        {
            get
            {
                return "FieldValueMap";
            }
        }

    }
}
