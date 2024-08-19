using MigrationTools.Tools.Infrastructure;
namespace MigrationTools.Tools
{
    /// <summary>
    /// Need to create a Tag based on a field value? Just create a regex match and choose how to populate the target.
    /// </summary>
    /// <status>ready</status>
    /// <processingtarget>Work Item Field</processingtarget>
    public class FieldValuetoTagMapOptions : IFieldMapOptions
    {
        public string WorkItemTypeName { get; set; }
        public string sourceField { get; set; }
        public string pattern { get; set; }
        public string formatExpression { get; set; }

        public string FieldMap
        {
            get
            {
                return "FieldValuetoTagMap";
            }
        }

        public void SetExampleConfigDefaults()
        {
            WorkItemTypeName = "*";
            sourceField = "System.Status";
            pattern = "(Active|Resolved)";
            formatExpression = "Status: {0}";
        }
    }
}