namespace MigrationTools._EngineV1.Configuration.FieldMap
{
    public class FieldBlankMapConfig : IFieldMapConfig
    {
        public string WorkItemTypeName { get; set; }
        public string targetField { get; set; }

        public string FieldMap
        {
            get
            {
                return "FieldBlankMap";
            }
        }
    }
}