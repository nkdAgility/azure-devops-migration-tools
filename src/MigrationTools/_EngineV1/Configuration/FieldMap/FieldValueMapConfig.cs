using System.Collections.Generic;

namespace MigrationTools._EngineV1.Configuration.FieldMap
{
    /// <summary>
    /// Need to map not just the field but also values? This is the default value mapper.
    /// </summary>
    /// <status>ready</status>
    /// <processingtarget>Work Item Field</processingtarget>
    public class FieldValueMapConfig : IFieldMapConfig
    {
        public string WorkItemTypeName { get; set; }
        public string sourceField { get; set; }
        public string targetField { get; set; }
        public string defaultValue { get; set; }
        public Dictionary<string, string> valueMapping { get; set; }

        public string FieldMap
        {
            get
            {
                return "FieldValueMap";
            }
        }

        public void SetExampleConfigDefaults()
        {
            WorkItemTypeName = "*";
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