using System.Collections.Generic;

namespace MigrationTools.Configuration.Processing
{
    public class TeamMigrationConfig : IProcessorConfig
    {
        /// <inheritdoc />
        public bool Enabled { get; set; }

        public bool PrefixProjectToNodes { get; set; }

        public bool EnableTeamSettingsMigration { get; set; }

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