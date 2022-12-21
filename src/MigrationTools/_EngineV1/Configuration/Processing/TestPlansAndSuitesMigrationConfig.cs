using System.Collections.Generic;

namespace MigrationTools._EngineV1.Configuration.Processing
{
    public class TestPlansAndSuitesMigrationConfig : IProcessorConfig
    {
        public bool PrefixProjectToNodes { get; set; }
        public bool Enabled { get; set; }
        public string OnlyElementsWithTag { get; set; }
        public string TestPlanQueryBit { get; set; }
        public bool RemoveAllLinks { get; set; }
        public int MigrationDelay { get; set; }

        public bool UseCommonNodeStructureEnricherConfig { get; set; }
        public string[] NodeBasePaths { get; set; }
        public Dictionary<string, string> AreaMaps { get; set; }
        public Dictionary<string, string> IterationMaps { get; set; }

        public string Processor
        {
            get { return "TestPlandsAndSuitesMigrationContext"; }
        }

        /// <summary>
        /// Remove Invalid Links, see https://github.com/nkdAgility/azure-devops-migration-tools/issues/178
        /// </summary>
        public bool RemoveInvalidTestSuiteLinks { get; set; }

        public bool FilterCompleted { get; set; }

        /// <inheritdoc />
        public bool IsProcessorCompatible(IReadOnlyList<IProcessorConfig> otherProcessors)
        {
            return true;
        }

        public TestPlansAndSuitesMigrationConfig()
        {
            MigrationDelay = 0;
            RemoveAllLinks = false;
        }
    }
}