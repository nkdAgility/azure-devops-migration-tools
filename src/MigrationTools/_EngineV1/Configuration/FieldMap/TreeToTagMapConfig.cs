﻿namespace MigrationTools._EngineV1.Configuration.FieldMap
{
    /// <summary>
    /// Need to clear out those nasty Area tree hierarchies? This creates Tags for each node in the Area Path...
    /// </summary>
    /// <status>ready</status>
    /// <processingtarget>Work Item Field</processingtarget>
    public class TreeToTagMapConfig : IFieldMapConfig
    {
        public string WorkItemTypeName { get; set; }
        public int toSkip { get; set; }
        public int timeTravel { get; set; }

        public string FieldMap
        {
            get
            {
                return "TreeToTagFieldMap";
            }
        }

        public void SetExampleConfigDefaults()
        {
            WorkItemTypeName = "*";
            toSkip = 2;
            timeTravel = 0;
        }
    }
}