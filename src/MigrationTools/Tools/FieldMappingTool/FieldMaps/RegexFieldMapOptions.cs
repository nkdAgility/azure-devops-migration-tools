using System.Collections.Generic;
using MigrationTools.Tools.Infrastructure;
namespace MigrationTools.Tools
{
    /// <summary>
    ///  I just need that bit of a field... need to send "2016.2" to two fields, one for year and one for release? Done.
    /// </summary>
    /// <status>ready</status>
    /// <processingtarget>Work Item Field</processingtarget>
    public class RegexFieldMapOptions : FieldMapOptions
    {
        /// <summary>
        /// Gets or sets the name of the source field to read data from and apply regex pattern matching.
        /// </summary>
        public string sourceField { get; set; }
        
        /// <summary>
        /// Gets or sets the name of the target field to write the regex-transformed data to.
        /// </summary>
        public string targetField { get; set; }
        
        /// <summary>
        /// Gets or sets the regular expression pattern to match against the source field value.
        /// </summary>
        public string pattern { get; set; }
        
        /// <summary>
        /// Gets or sets the replacement pattern that defines how matched groups should be used to construct the target value.
        /// </summary>
        public string replacement { get; set; }

        public void SetExampleConfigDefaults()
        {
            ApplyTo = new List<string>() { "*" };
            sourceField = "Custom.MyVersion";
            targetField = "Custom.MyVersionYearOnly";
            pattern = "([0-9]{4})";
            replacement = "$1";
        }
    }
}