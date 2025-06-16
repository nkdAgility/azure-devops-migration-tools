using System.Collections.Generic;
using MigrationTools.Tools.Infrastructure;

namespace MigrationTools.Tools
{
    /// <summary>
    /// Allows you to set an already populated field to Null. This will only work with fields that support null.
    /// </summary>
    /// <status>ready</status>
    /// <processingtarget>Work Item</processingtarget>
    public class FieldClearMapOptions : FieldMapOptions
    {
        /// <summary>
        /// Gets or sets the name of the target field to be cleared/set to null during work item migration.
        /// </summary>
        public string targetField { get; set; }

        public void SetExampleConfigDefaults()
        {
            ApplyTo = new List<string>() { "*" };
            targetField = "System.Description";
        }
    }
}