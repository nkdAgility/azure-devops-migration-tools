using System;
using System.Collections.Generic;

namespace MigrationTools.Core.Configuration.Processing
{
    public class TestPlansAndSuitesMigrationConfig : IProcessorConfig
    {
        public bool PrefixProjectToNodes { get; set; }
        public bool Enabled { get; set; }
        public string OnlyElementsWithTag { get; set; }
        public string TestPlanQueryBit { get; set; }

        public string Processor
        {
            get { return "TestPlandsAndSuitesMigrationContext"; }
        }

        /// <summary>
        /// Remove Invalid Links, see https://github.com/nkdAgility/azure-devops-migration-tools/issues/178
        /// </summary>
        public bool RemoveInvalidTestSuiteLinks { get; set; }

        /// <inheritdoc />
        public bool IsProcessorCompatible(IReadOnlyList<IProcessorConfig> otherProcessors)
        {
            return true;
        }

        public TestPlansAndSuitesMigrationConfig()
        {
        }
    }
}
