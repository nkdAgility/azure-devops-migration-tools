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
        public int toSkip { get; set; }
        public int timeTravel { get; set; }

        public void SetExampleConfigDefaults()
        {
            ApplyTo = new List<string>() { "*" };
            toSkip = 2;
            timeTravel = 0;
        }
    }
}