using System;
using System.Collections.Generic;
using MigrationTools.Options;
using MigrationTools.Tools.Infrastructure;

namespace MigrationTools.Tools
{
    /// <summary>
    /// Want to take a field and convert its value to a tag? Done...
    /// </summary>
    /// <status>ready</status>
    /// <processingtarget>Work Item Field</processingtarget>
    public class FieldToTagFieldMapOptions : FieldMapOptions
    {
        public string sourceField { get; set; }
        public string formatExpression { get; set; }


        public void SetExampleConfigDefaults()
        {
            ApplyTo = new List<string>() { "*" };
            sourceField = "Custom.ProjectName";
            formatExpression = "Project: {0}";
        }
    }
}