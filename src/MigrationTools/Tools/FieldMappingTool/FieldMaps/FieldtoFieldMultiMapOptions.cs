using System.Collections.Generic;
using MigrationTools.Tools.Infrastructure;

namespace MigrationTools.Tools
{
    /// <summary>
    /// Want to setup a bunch of field maps in a single go. Use this shortcut!
    /// </summary>
    /// <status>ready</status>
    /// <processingtarget>Work Item Field</processingtarget>
    public class FieldToFieldMultiMapOptions : FieldMapOptions
    {
        public string WorkItemTypeName { get; set; }
        public Dictionary<string, string> SourceToTargetMappings { get; set; }

        public void SetExampleConfigDefaults()
        {
            WorkItemTypeName = "*";
            SourceToTargetMappings = new Dictionary<string, string>
            {
                { "Custom.Field1", "Custom.Field4" },
                { "Custom.Field2", "Custom.Field5" },
                { "Custom.Field3", "Custom.Field6" }
            };
        }
    }
}