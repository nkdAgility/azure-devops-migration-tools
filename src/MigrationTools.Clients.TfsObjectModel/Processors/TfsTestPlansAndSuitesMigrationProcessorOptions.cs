using System.Collections.Generic;
using MigrationTools.Enrichers;
using MigrationTools.Processors;
using MigrationTools.Processors.Infrastructure;
using System;

namespace MigrationTools._EngineV1.Configuration.Processing
{
    /// <summary>
    /// Configuration options for the TFS Test Plans and Suites Migration Processor that migrates test plans, test suites, and their associated test cases between TFS/Azure DevOps projects.
    /// </summary>
    public class TfsTestPlansAndSuitesMigrationProcessorOptions : ProcessorOptions
    {
        /// <summary>
        /// Only plans, suites and Test Cases whose tags contain this text are migrated. The match is a substring of the tags text, so `move` also matches `remove`. A plan or suite under the root suite that does not match is skipped with everything below it. Leave it empty or unset to migrate everything.
        /// </summary>
        /// <default>null</default>
        public string OnlyElementsWithTag { get; set; }

        /// <summary>
        /// Filtering conditions to decide whether to migrate a test plan or not. This is not WIQL: when provided, this partial query is added after `Select * From TestPlan Where` when selecting test plans. Among filtering options, `AreaPath`, `PlanName` and `PlanState` are known to work. There is unfortunately no documentation regarding the available fields. Leave it empty to select every plan.
        /// </summary>
        /// <default>null</default>
        public string TestPlanQuery { get; set; }

        /// <summary>
        /// If `true`, removes every link from each Test Plan the processor creates in the target. Plans that already exist in the target are not changed.
        /// </summary>
        /// <default>false</default>
        public bool RemoveAllLinks { get; set; }

        /// <summary>
        /// Milliseconds to wait before processing each Test Suite. Use it to slow the run down when the target throttles requests. `0` means no delay.
        /// </summary>
        /// <default>0</default>
        public int MigrationDelay { get; set; }

        /// <summary>
        /// If `true`, removes links whose URI is not valid from each Test Plan the processor creates in the target, and logs each one. If `false`, such links are kept and a warning is logged, and the save can fail with "The URL specified has a potentially unsafe URL protocol". See https://github.com/nkdAgility/azure-devops-migration-tools/issues/178.
        /// </summary>
        /// <default>false</default>
        public bool RemoveInvalidTestSuiteLinks { get; set; }

        /// <summary>
        /// If `true`, skips each source Test Plan whose name already exists in the target, so plans migrated by an earlier run are not processed again.
        /// </summary>
        /// <default>false</default>
        public bool FilterCompleted { get; set; }

        /// <summary>
        /// Migrates only the Test Plans with these IDs. Pass the test plan IDs as an array, for example `[123, 456, 789]`. Leave it empty to migrate every plan selected by `TestPlanQuery`.
        /// </summary>
        /// <default>[]</default>
        public int[] TestPlanIds { get; set; } = Array.Empty<int>();

        public TfsTestPlansAndSuitesMigrationProcessorOptions()
        {
            MigrationDelay = 0;
            RemoveAllLinks = false;
        }
    }
}
