using MigrationTools.Tools.Infrastructure;

namespace MigrationTools.Tools
{
    /// <summary>
    /// Sets a field on the `target` to b a specific value.
    /// </summary>
    /// <status>ready</status>
    /// <processingtarget>Work Item Field</processingtarget>
    public class FieldLiteralMapOptions : FieldMapOptions
    {

        public string WorkItemTypeName { get; set; }

        public string targetField { get; set; }

        public string value { get; set; }

        public void SetExampleConfigDefaults()
        {
            WorkItemTypeName = "*";
            targetField = "System.Status";
            value = "New";
        }
    }
}