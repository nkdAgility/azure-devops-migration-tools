namespace MigrationTools._EngineV1.Configuration.FieldMap
{
    /// <summary>
    /// Allows you to skip populating an existing field. Value in target with be reset to its OriginalValue.
    /// </summary>
    /// <status>ready</status>
    /// <processingtarget>Work Item</processingtarget>
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

        public void SetExampleConfigDefaults()
        {
            WorkItemTypeName = "*";
            targetField = "System.Description";
        }
    }
}