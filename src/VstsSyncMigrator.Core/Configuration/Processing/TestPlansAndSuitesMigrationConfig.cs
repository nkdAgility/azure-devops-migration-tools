using System;
using System.Collections.Generic;

namespace VstsSyncMigrator.Engine.Configuration.Processing
{
    public class TestPlansAndSuitesMigrationConfig : ITfsProcessingConfig
    {
        public bool PrefixProjectToNodes { get; set; }
        public bool Enabled { get; set; }
        public string OnlyElementsWithTag { get; set; }

        public Type Processor
        {
            get { return typeof(TestPlandsAndSuitesMigrationContext); }
        }

        /// <inheritdoc />
        public bool IsProcessorCompatible(IReadOnlyList<ITfsProcessingConfig> otherProcessors)
        {
            return true;
        }
    }
}