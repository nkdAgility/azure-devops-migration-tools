using System;
using System.Collections.Generic;

namespace VstsSyncMigrator.Engine.Configuration.Processing
{
    public class TestPlansAndSuitesMigrationConfig : ITfsProcessingConfig
    {
        public bool PrefixProjectToNodes { get; set; }
        public bool Enabled { get; set; }
        public string OnlyElementsWithTag { get; set; }
        public string TestPlanQueryBit { get; set; }

        public Type Processor
        {
            get { return typeof(TestPlandsAndSuitesMigrationContext); }
        }

        /// <summary>
        /// Remove Invalid Links, see https://github.com/nkdAgility/azure-devops-migration-tools/issues/178
        /// </summary>
        public bool RemoveInvalidTestSuiteLinks { get; set; }

        /// <inheritdoc />
        public bool IsProcessorCompatible(IReadOnlyList<ITfsProcessingConfig> otherProcessors)
        {
            return true;
        }
    }
}