namespace MigrationTools._EngineV1.Configuration.FieldMap
{
    /// <summary>
    /// Allows you to set an already populated field to Null. This will only work with fields that support null.
    /// </summary>
    /// <status>ready</status>
    /// <processingtarget>Work Item</processingtarget>
    public class FieldClearMapConfig : IFieldMapConfig
    {
        public string WorkItemTypeName { get; set; }
        public string targetField { get; set; }

        public string FieldMap
        {
            get
            {
                return "FieldClearMap";
            }
        }

        public void SetExampleConfigDefaults()
        {
            WorkItemTypeName = "*";
            targetField = "System.Description";
        }
    }
}