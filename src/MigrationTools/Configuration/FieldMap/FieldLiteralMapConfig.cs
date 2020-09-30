namespace MigrationTools.Configuration.FieldMap
{
    public class FieldLiteralMapConfig : IFieldMapConfig
    {
        public string FieldMap => "FieldLiteralMap";
        
        public string WorkItemTypeName { get; set; }

        public string targetField { get; set; }
        
        public string value { get; set; }
    }
}
