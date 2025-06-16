using System.Collections.Generic;
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
        /// <summary>
        /// Gets or sets the name of the target field that will be set to the specified literal value.
        /// </summary>
        public string targetField { get; set; }

        /// <summary>
        /// Gets or sets the literal value that will be assigned to the target field during migration.
        /// </summary>
        public string value { get; set; }

        public void SetExampleConfigDefaults()
        {
            ApplyTo = new List<string>() { "*" };
            targetField = "System.Status";
            value = "New";
        }
    }
}