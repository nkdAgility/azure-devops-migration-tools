using System.Collections.Generic;

namespace MigrationTools._EngineV1.Configuration.Processing
{
    public class TeamMigrationConfig : IProcessorConfig
    {
        /// <inheritdoc />
        public bool Enabled { get; set; }

        /// <summary>
        /// Prefix your iterations and areas with the project name. If you have enabled this in `NodeStructuresMigrationConfig` you must do it here too.
        /// </summary>
        /// <default>false</default>
        public bool PrefixProjectToNodes { get; set; }

        /// <summary>
        /// Migrate original team settings after their creation on target team project
        /// </summary>
        /// <default>true</default>
        public bool EnableTeamSettingsMigration { get; set; }

        /// <summary>
        /// Reset the target team settings to match the source if the team exists
        /// </summary>
        /// <default>true</default>
        public bool FixTeamSettingsForExistingTeams { get; set; }

        /// <inheritdoc />
        public string Processor
        {
            get { return "TeamMigrationContext"; }
        }

        /// <inheritdoc />
        public bool IsProcessorCompatible(IReadOnlyList<IProcessorConfig> otherProcessors)
        {
            return true;
        }

        public TeamMigrationConfig()
        {
            EnableTeamSettingsMigration = true;
        }
    }
}