using System.Collections.Generic;
using MigrationTools.Tools.Infrastructure;
namespace MigrationTools.Tools
{
    /// <summary>
    /// Need to create a Tag based on a field value? Just create a regex match and choose how to populate the target.
    /// </summary>
    /// <status>ready</status>
    /// <processingtarget>Work Item Field</processingtarget>
    public class FieldValuetoTagMapOptions : FieldMapOptions
    {
        public string sourceField { get; set; }
        public string pattern { get; set; }
        public string formatExpression { get; set; }

        public void SetExampleConfigDefaults()
        {
            ApplyTo = new List<string>() { "*" };
            sourceField = "System.Status";
            pattern = "(Active|Resolved)";
            formatExpression = "Status: {0}";
        }
    }
}