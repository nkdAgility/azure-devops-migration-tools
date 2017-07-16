using System;
using System.Collections.Generic;

namespace VstsSyncMigrator.Engine.Configuration.Processing
{
    public class TestRunsMigrationConfig : ITfsProcessingConfig
    {
        public string Status
        {
            get { return "Experimental"; }
        }

        public bool Enabled { get; set; }

        public Type Processor
        {
            get { return typeof(TestRunsMigrationContext); }
        }

        /// <inheritdoc />
        public bool IsProcessorCompatible(IReadOnlyList<ITfsProcessingConfig> otherProcessors)
        {
            return true;
        }
    }
}