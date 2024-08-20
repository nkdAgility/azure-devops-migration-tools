using System.Collections.Generic;
using MigrationTools.Tools.Infrastructure;

namespace MigrationTools.Tools
{
    /// <summary>
    /// Allows you to skip populating an existing field. Value in target with be reset to its OriginalValue.
    /// </summary>
    /// <status>ready</status>
    /// <processingtarget>Work Item</processingtarget>
    public class FieldSkipMapOptions : FieldMapOptions
    {
        public string targetField { get; set; }


        public void SetExampleConfigDefaults()
        {
            ApplyTo = new List<string>() { "*" };
            targetField = "System.Description";
        }
    }
}