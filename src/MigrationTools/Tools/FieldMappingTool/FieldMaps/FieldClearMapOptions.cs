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
        public string targetField { get; set; }

        public void SetExampleConfigDefaults()
        {
            ApplyTo = new List<string>() { "*" };
            targetField = "System.Description";
        }
    }
}