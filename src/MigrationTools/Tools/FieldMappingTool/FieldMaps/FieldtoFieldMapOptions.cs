
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using MigrationTools.Tools.Infrastructure;

namespace MigrationTools.Tools
{

    [JsonConverter(typeof(StringEnumConverter))]
    public enum FieldMapMode
    {
        SourceToTarget = 0,
        TargetToTarget = 1
    }


    /// <summary>
    /// Just want to map one field to another? This is the one for you.
    /// </summary>
    /// <status>ready</status>
    /// <processingtarget>Work Item Field</processingtarget>
    public class FieldToFieldMapOptions : FieldMapOptions
    {

        /// <summary>
        /// Gets or sets the mode of field mapping to be applied during migration. SourceToTarget will get data from the source system and apply it to the target. TargetToTarget will move data between the target fields.
        /// </summary>
        /// <default>SourceToTarget</default>
        public FieldMapMode fieldMapMode { get; set; } = FieldMapMode.SourceToTarget;

        /// <summary>
        /// Gets or sets the name of the source field to copy data from during migration.
        /// </summary>
        public string sourceField { get; set; }

        /// <summary>
        /// Gets or sets the name of the target field to copy data to during migration.
        /// </summary>
        public string targetField { get; set; }

        /// <summary>
        /// Gets or sets the default value to use when the source field is empty or null.
        /// </summary>
        public string defaultValue { get; set; }

        public void SetExampleConfigDefaults()
        {
            ApplyTo = new List<string>() { "*" };
            sourceField = "System.StackRank";
            targetField = "System.Rank";
            defaultValue = "1000";
            fieldMapMode = FieldMapMode.SourceToTarget;
        }
    }
}
