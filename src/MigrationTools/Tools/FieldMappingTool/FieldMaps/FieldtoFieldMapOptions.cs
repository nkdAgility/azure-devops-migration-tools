
using MigrationTools.Tools.Infrastructure;

namespace MigrationTools.Tools
{
    /// <summary>
    /// Just want to map one field to another? This is the one for you.
    /// </summary>
    /// <status>ready</status>
    /// <processingtarget>Work Item Field</processingtarget>
    public class FieldtoFieldMapOptions : FieldMapOptions
    {
        public string WorkItemTypeName { get; set; }
        public string sourceField { get; set; }
        public string targetField { get; set; }
        public string defaultValue { get; set; }

        public void SetExampleConfigDefaults()
        {
            WorkItemTypeName = "*";
            sourceField = "System.StackRank";
            targetField = "System.Rank";
            defaultValue = "1000";
        }
    }
}