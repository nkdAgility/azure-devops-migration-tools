using System.Collections.Generic;
using MigrationTools.Tools.Infrastructure;

namespace MigrationTools.Tools
{
    /// <summary>
    /// Need to clear out those nasty Area tree hierarchies? This creates Tags for each node in the Area Path...
    /// </summary>
    /// <status>ready</status>
    /// <processingtarget>Work Item Field</processingtarget>
    public class TreeToTagFieldMapOptions : FieldMapOptions
    {
        /// <summary>
        /// Gets or sets the number of levels to skip from the root when converting area path hierarchy to tags. For example, if set to 2, "ProjectName\Level1\Level2\Level3" would skip "ProjectName\Level1" and start from "Level2".
        /// </summary>
        public int toSkip { get; set; }
        
        /// <summary>
        /// Gets or sets the number of months to travel back in time when looking up historical area path values. Use 0 for current values.
        /// </summary>
        public int timeTravel { get; set; }

        public void SetExampleConfigDefaults()
        {
            ApplyTo = new List<string>() { "*" };
            toSkip = 2;
            timeTravel = 0;
        }
    }
}