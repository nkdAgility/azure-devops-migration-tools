namespace MigrationTools.Configuration.FieldMap
{
    public class FieldtoFieldMapConfig : IFieldMapConfig
    {
        public string WorkItemTypeName { get; set; }
        public string sourceField { get; set; }
        public string targetField { get; set; }
        public string defaultValue { get; set; }

        public string FieldMap
        {
            get
            {
                return "FieldToFieldMap";
            }
        }
    }
}