
namespace MigrationTools._EngineV1.Configuration.FieldMap
{
    /// <summary>
    /// Just want to map one field to another? This is the one for you.
    /// </summary>
    /// <status>ready</status>
    /// <processingtarget>Work Item Field</processingtarget>
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

        public void SetExampleConfigDefaults()
        {
            WorkItemTypeName = "*";
            sourceField = "System.StackRank";
            targetField = "System.Rank";
            defaultValue = "1000";
        }
    }
}