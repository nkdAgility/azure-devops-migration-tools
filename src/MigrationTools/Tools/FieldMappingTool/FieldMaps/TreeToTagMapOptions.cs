using MigrationTools.Tools.Infrastructure;

namespace MigrationTools.Tools
{
    /// <summary>
    /// Need to clear out those nasty Area tree hierarchies? This creates Tags for each node in the Area Path...
    /// </summary>
    /// <status>ready</status>
    /// <processingtarget>Work Item Field</processingtarget>
    public class TreeToTagMapOptions : FieldMapOptions
    {
        public string WorkItemTypeName { get; set; }
        public int toSkip { get; set; }
        public int timeTravel { get; set; }

        public void SetExampleConfigDefaults()
        {
            WorkItemTypeName = "*";
            toSkip = 2;
            timeTravel = 0;
        }
    }
}