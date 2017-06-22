using System;
using System.Collections.Generic;

namespace VstsSyncMigrator.Engine.Configuration.Processing
{
    public class TestVeriablesMigrationConfig : ITfsProcessingConfig
    {
        public bool Enabled { get; set; }

        public Type Processor
        {
            get { return typeof(TestVeriablesMigrationContext); }
        }

        /// <inheritdoc />
        public bool IsProcessorCompatible(IReadOnlyList<ITfsProcessingConfig> otherProcessors)
        {
            return true;
        }
    }
}