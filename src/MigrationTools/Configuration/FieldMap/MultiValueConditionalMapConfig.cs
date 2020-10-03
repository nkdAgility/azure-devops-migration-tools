using System.Collections.Generic;

namespace MigrationTools.Configuration.FieldMap
{
    public class MultiValueConditionalMapConfig : IFieldMapConfig
    {
        public string WorkItemTypeName { get; set; }
        public Dictionary<string, string> sourceFieldsAndValues { get; set; }
        public Dictionary<string, string> targetFieldsAndValues { get; set; }
        public string FieldMap
        {
            get
            {
                return "MultiValueConditionalMap";
            }
        }
    }
}
