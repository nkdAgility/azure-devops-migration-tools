using System.Collections.Generic;
using MigrationTools.Tools.Infrastructure;

namespace MigrationTools.Tools
{
    public class TfsTeamSettingsToolOptions : ToolOptions, ITfsTeamSettingsToolOptions
    {
        /// <summary>
        /// Migrate original team settings after their creation on target team project
        /// </summary>
        /// <default>false</default>
        public bool MigrateTeamSettings { get; set; }

        /// <summary>
        /// Reset the target team settings to match the source if the team exists
        /// </summary>
        /// <default>false</default>
        public bool UpdateTeamSettings { get; set; }

        /// <summary>
        /// Migrate original team member capacities after their creation on the target team project. Note: It will only migrate team member capacity if the team member with same display name exists on the target collection otherwise it will be ignored.
        /// </summary>
        /// <default>false</default>
        public bool MigrateTeamCapacities { get; set; }

        /// <summary>
        /// Use user mapping file from TfsTeamSettingsTool when matching users when migrating capacities.
        /// By default, users in source are matched in target users by current display name. When this is set to `true`,
        /// users are matched also by mapped name from user mapping file.
        /// </summary>
        public bool UseUserMapping { get; set; }

        /// <summary>
        /// List of Teams to process. If this is `null` then all teams will be processed.
        /// </summary>
        public List<string> Teams { get; set; }
    }

    public interface ITfsTeamSettingsToolOptions
    {
        public bool MigrateTeamSettings { get; set; }

        public bool UpdateTeamSettings { get; set; }

        public bool MigrateTeamCapacities { get; set; }

        public List<string> Teams { get; set; }

        public bool UseUserMapping { get; set; }
    }
}
