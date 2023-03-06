namespace MigrationTools._EngineV1.Configuration.FieldMap
{
    /// <summary>
    /// Allows you to blank an already populated field
    /// </summary>
    /// <status>ready</status>
    /// <processingtarget>Work Item Field</processingtarget>
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