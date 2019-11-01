using System;
using System.Collections.Generic;

namespace VstsSyncMigrator.Engine.Configuration.Processing
{
    public class TeamMigrationConfig : ITfsProcessingConfig
    {
        /// <inheritdoc />
        public bool Enabled { get; set; }

        public bool PrefixProjectToNodes { get; set; }

        public bool EnableTeamSettingsMigration { get; set; }

        /// <inheritdoc />
        public Type Processor
        {
            get { return typeof(TeamMigrationContext); }
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