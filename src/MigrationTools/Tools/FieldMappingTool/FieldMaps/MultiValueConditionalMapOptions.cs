using System.Collections.Generic;

namespace MigrationTools._EngineV1.Configuration.FieldMap
{
    /// <summary>
    /// ??? If you know how to use this please send a PR :)
    /// </summary>
    /// <status>ready</status>
    /// <processingtarget>Work Item Field</processingtarget>
    public class MultiValueConditionalMapOptions : IFieldMapConfig
    {
        public string WorkItemTypeName { get; set; }
        public Dictionary<string, string> sourceFieldsAndValues { get; set; }
        public Dictionary<string, string> targetFieldsAndValues { get; set; }

        public string FieldMap
        {
            get
            {
                return "MultiValueConditionalMap";
            }
        }

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