using System.Collections.Generic;
using MigrationTools.Tools.Infrastructure;

namespace MigrationTools.Tools
{
    /// <summary>
    /// ??? If you know how to use this please send a PR :)
    /// </summary>
    /// <status>ready</status>
    /// <processingtarget>Work Item Field</processingtarget>
    public class MultiValueConditionalMapOptions : FieldMapOptions
    {
        public string WorkItemTypeName { get; set; }
        public Dictionary<string, string> sourceFieldsAndValues { get; set; }
        public Dictionary<string, string> targetFieldsAndValues { get; set; }

        public void SetExampleConfigDefaults()
        {
            WorkItemTypeName = "*";
            sourceFieldsAndValues = new Dictionary<string, string>
            {
                { "Something", "SomethingElse" }
            };
            targetFieldsAndValues = new Dictionary<string, string>
            {
                { "Something", "SomethingElse" }
            };

        }
    }
}