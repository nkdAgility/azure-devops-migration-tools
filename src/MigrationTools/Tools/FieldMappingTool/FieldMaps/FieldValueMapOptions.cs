using System.Collections.Generic;
using MigrationTools.Tools.Infrastructure;

namespace MigrationTools.Tools
{
    /// <summary>
    /// Need to map not just the field but also values? This is the default value mapper.
    /// </summary>
    /// <status>ready</status>
    /// <processingtarget>Work Item Field</processingtarget>
    public class FieldValueMapOptions : FieldMapOptions
    {
        public string sourceField { get; set; }
        public string targetField { get; set; }
        public string defaultValue { get; set; }
        public Dictionary<string, string> valueMapping { get; set; }

        public void SetExampleConfigDefaults()
        {
            ApplyTo = new List<string>() { "*" };
            sourceField = "System.Status";
            targetField = "System.Status";
            defaultValue = "New";
            valueMapping = new Dictionary<string, string>
            {
                { "New", "New" },
                { "Active", "Committed" },
                { "Resolved", "Committed" },
                { "Closed", "Done" }
            };
        }
    }
}