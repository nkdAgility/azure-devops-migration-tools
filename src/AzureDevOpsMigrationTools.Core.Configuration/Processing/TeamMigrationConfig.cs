using System;
using System.Collections.Generic;

namespace AzureDevOpsMigrationTools.Core.Configuration.Processing
{
    public class TeamMigrationConfig : ITfsProcessingConfig
    {
        /// <inheritdoc />
        public bool Enabled { get; set; }

        public bool PrefixProjectToNodes { get; set; }

        public bool EnableTeamSettingsMigration { get; set; }

        /// <inheritdoc />
        public string Processor
        {
            get { return "TeamMigrationContext"; }
        }

        /// <inheritdoc />
        public bool IsProcessorCompatible(IReadOnlyList<ITfsProcessingConfig> otherProcessors)
        {
            return true;
        }

        public TeamMigrationConfig()
        {
            EnableTeamSettingsMigration = true;
        }
    }
}