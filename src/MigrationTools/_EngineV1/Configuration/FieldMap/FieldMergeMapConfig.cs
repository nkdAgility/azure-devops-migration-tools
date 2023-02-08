using System.Collections.Generic;

namespace MigrationTools._EngineV1.Configuration.FieldMap
{
    public class FieldMergeMapConfig : IFieldMapConfig
    {
        public string WorkItemTypeName { get; set; }
        public List<string> sourceFields { get; set; }
        public string targetField { get; set; }
        public string formatExpression { get; set; }
        public string doneMatch { get; set; } = "##DONE##"; //Depricated TODO remove

        public string FieldMap
        {
            get
            {
                return "FieldMergeMap";
            }
        }
    }
}