using System;
using System.Collections.Generic;

namespace VstsSyncMigrator.Engine.Configuration.Processing
{
    public class TeamMigrationConfig : ITfsProcessingConfig
    {
        public bool Enabled { get; set; }

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