using System.Collections.Generic;
using MigrationTools.Enrichers;
using MigrationTools.Processors;
using MigrationTools.Processors.Infrastructure;

namespace MigrationTools._EngineV1.Configuration.Processing
{
    public class TfsTestPlansAndSuitesMigrationProcessorOptions : ProcessorOptions
    {
        /// <summary>
        /// The tag name that is present on all elements that must be migrated. If this option isn't present this processor will migrate all.
        /// </summary>
        /// <default>`String.Empty`</default>
        public string OnlyElementsWithTag { get; set; }

        /// <summary>
        /// Filtering conditions to decide whether to migrate a test plan or not. When provided, this partial query is added after `Select * From TestPlan Where` when selecting test plans. Among filtering options, `AreaPath`, `PlanName` and `PlanState` are known to work. There is unfortunately no documentation regarding the available fields.
        /// </summary>
        /// <default>`String.Empty`</default>
        public string TestPlanQuery { get; set; }

        /// <summary>
        /// ??Not sure what this does. Check code.
        /// </summary>
        /// <default>false</default>
        public bool RemoveAllLinks { get; set; }

        /// <summary>
        /// ??Not sure what this does. Check code.
        /// </summary>
        /// <default>0</default>
        public int MigrationDelay { get; set; }

        /// <summary>
        /// Indicates whether the configuration for node structure transformation should be taken from the common enricher configs. Otherwise the configuration elements below are used
        /// </summary>
        /// <default>false</default>

        /// <summary>
        /// Remove Invalid Links, see https://github.com/nkdAgility/azure-devops-migration-tools/issues/178
        /// </summary>
        public bool RemoveInvalidTestSuiteLinks { get; set; }

        public bool FilterCompleted { get; set; }

        public TfsTestPlansAndSuitesMigrationProcessorOptions()
        {
            MigrationDelay = 0;
            RemoveAllLinks = false;
        }
    }
}
