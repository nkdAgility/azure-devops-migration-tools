
using System.Collections.Generic;
using MigrationTools.Tools.Infrastructure;

namespace MigrationTools.Tools
{
    /// <summary>
    /// Just want to map one field to another? This is the one for you.
    /// </summary>
    /// <status>ready</status>
    /// <processingtarget>Work Item Field</processingtarget>
    public class FieldToFieldMapOptions : FieldMapOptions
    {
        public string sourceField { get; set; }
        public string targetField { get; set; }
        public string defaultValue { get; set; }

        public void SetExampleConfigDefaults()
        {
            ApplyTo = new List<string>() { "*" };
            sourceField = "System.StackRank";
            targetField = "System.Rank";
            defaultValue = "1000";
        }
    }
}