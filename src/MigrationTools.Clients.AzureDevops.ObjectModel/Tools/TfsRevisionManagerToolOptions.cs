using System;
using MigrationTools.Enrichers;
using MigrationTools.Tools.Infrastructure;

namespace MigrationTools.Tools
{
    public class TfsRevisionManagerToolOptions : ToolOptions
    {
        public const string ConfigurationSectionName = "MigrationTools:CommonTools:TfsRevisionManagerTool";

        /// <summary>
        /// You can choose to migrate the tip only (a single write) or all of the revisions (many writes).
        /// If you are setting this to `false` to migrate only the tip then you should set `BuildFieldTable` to `true`.
        /// </summary>
        /// <default>true</default>
        public bool ReplayRevisions { get; set; }

        /// <summary>
        /// Sets the maximum number of revisions that will be migrated. "First + Last N = Max".
        /// If this was set to 5 and there were 10 revisions you would get the first 1 (creation) and the latest 4 migrated.
        /// </summary>
        /// <default>0</default>
        public int MaxRevisions { get; set; }
    }
}