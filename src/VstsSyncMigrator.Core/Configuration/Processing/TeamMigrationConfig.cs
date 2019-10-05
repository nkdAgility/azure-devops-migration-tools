using System;
using System.Collections.Generic;

namespace VstsSyncMigrator.Engine.Configuration.Processing
{
    public class TeamMigrationConfig : ITfsProcessingConfig
    {
        /// <inheritdoc />
        public bool Enabled { get; set; }

        public bool PrefixProjectToNodes { get; set; }

        /// <summary>
        ///     Do we automatically migrate Team Settings after Team creation ?
        /// </summary>
        public bool EnableTeamSettingsMigration { get; set; } = false;

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
    }
}