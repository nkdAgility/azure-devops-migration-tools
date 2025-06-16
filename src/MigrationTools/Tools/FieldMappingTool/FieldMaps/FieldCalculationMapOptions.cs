using System.Collections.Generic;
using MigrationTools.Tools.Infrastructure;

namespace MigrationTools.Tools
{
    /// <summary>
    /// Performs mathematical calculations on numeric fields using NCalc expressions during migration.
    /// </summary>
    /// <status>ready</status>
    /// <processingtarget>Work Item Field</processingtarget>
    public class FieldCalculationMapOptions : FieldMapOptions
    {
        /// <summary>
        /// Gets or sets the NCalc expression to evaluate. Variables in the expression should be enclosed in square brackets (e.g., "[x]*2").
        /// </summary>
        /// <default>null</default>
        public string expression { get; set; }

        /// <summary>
        /// Gets or sets a dictionary mapping variable names used in the expression to source field reference names.
        /// </summary>
        /// <default>{}</default>
        public Dictionary<string, string> parameters { get; set; }

        /// <summary>
        /// Gets or sets the target field reference name where the calculated result will be stored.
        /// </summary>
        /// <default>null</default>
        public string targetField { get; set; }

        /// <summary>
        /// Sets example configuration defaults for documentation purposes.
        /// </summary>
        public void SetExampleConfigDefaults()
        {
            ApplyTo = new List<string>() { "SomeWorkItemType" };
            expression = "[x]*2";
            parameters = new Dictionary<string, string>
            {
                { "x", "Custom.FieldB" }
            };
            targetField = "Custom.FieldC";
        }

        /// <summary>
        /// Initializes a new instance of the FieldCalculationMapOptions class.
        /// </summary>
        public FieldCalculationMapOptions()
        {
            parameters = new Dictionary<string, string>();
        }
    }
}