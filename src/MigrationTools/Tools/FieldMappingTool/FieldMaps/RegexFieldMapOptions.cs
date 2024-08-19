namespace MigrationTools._EngineV1.Configuration.FieldMap
{
    /// <summary>
    ///  I just need that bit of a field... need to send "2016.2" to two fields, one for year and one for release? Done.
    /// </summary>
    /// <status>ready</status>
    /// <processingtarget>Work Item Field</processingtarget>
    public class RegexFieldMapOptions : IFieldMapOptions
    {
        public string WorkItemTypeName { get; set; }
        public string sourceField { get; set; }
        public string targetField { get; set; }
        public string pattern { get; set; }
        public string replacement { get; set; }

        public string FieldMap
        {
            get
            {
                return "RegexFieldMap";
            }
        }

        public void SetExampleConfigDefaults()
        {
            WorkItemTypeName = "*";
            sourceField = "Custom.MyVersion";
            targetField = "Custom.MyVersionYearOnly";
            pattern = "([0-9]{4})";
            replacement = "$1";
        }
    }
}