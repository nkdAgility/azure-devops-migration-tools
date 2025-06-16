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
        /// <summary>
        /// Gets or sets the name of the source field to read values from during migration.
        /// </summary>
        public string sourceField { get; set; }
        
        /// <summary>
        /// Gets or sets the name of the target field to write mapped values to during migration.
        /// </summary>
        public string targetField { get; set; }
        
        /// <summary>
        /// Gets or sets the default value to use when no mapping is found for the source field value.
        /// </summary>
        public string defaultValue { get; set; }
        
        /// <summary>
        /// Gets or sets the dictionary that maps source field values to target field values. Key is the source value, value is the target value.
        /// </summary>
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