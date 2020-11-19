using System;
using MigrationTools.Endpoints;

namespace MigrationTools.Processors
{
    /// <summary>
    /// The `TfsPlansAndSuitsProcessor` will migrate test suites and test plans.
    /// </summary>
    public class TfsPlansAndSuitsProcessorOptions : ProcessorOptions
    {
        /// <summary>
        /// Filter the list of Test Plans to process by those that have been completed
        /// </summary>
        /// <default>true</default>
        public bool FilterCompleted { get; set; }

        /// <summary>
        /// The tag name that is present on all elements that must be migrated. If this option isn't present this processor will migrate all.
        /// </summary>
        /// <default>null</default>
        public string OnlyElementsWithTag { get; set; }

        /// <summary>
        /// Prefix the nodes with the new project name.
        /// </summary>
        /// <default>false</default>
        public bool PrefixProjectToNodes { get; set; }

        /// <summary>
        /// This option will skip invalid links. That is usually happened if in a test plan is a link to a tfvc changeset in the test case.<br>If that option is false you get an error if you have unsaved links like this in your test plan. If it true you only get a warning. https://github.com/nkdAgility/azure-devops-migration-tools/issues/178
        /// </summary>
        /// <default>false</default>
        public bool RemoveInvalidTestSuiteLinks { get; set; }

        /// <summary>
        /// Query to be used to retrieve test plans. Do not change the default unless you really know what you are doing.
        /// </summary>
        public string TestPlanQuery { get; set; }

        /// <summary>
        /// Query to be used to retrieve test runs. Do not change the default unless you really know what you are doing.
        /// </summary>
        public string TestRunQuery { get; set; }

        /// <summary>
        /// Query to be used to retrieve test configurations. Do not change the default unless you really know what you are doing.
        /// </summary>
        public string TestConfigurationQuery { get; set; }

        public override Type ToConfigure => typeof(TfsSharedQueryProcessor);

        public override string Processor => ToConfigure.Name;

        public override IProcessorOptions GetDefault()
        {
            return this;
        }

        public override void SetDefaults()
        {
            FilterCompleted = true;
            PrefixProjectToNodes = false;
            RemoveInvalidTestSuiteLinks = true;
            TestPlanQuery = "Select * From TestPlan";
            TestRunQuery = "Select * From TestRun";
            TestConfigurationQuery = "Select * From TestConfiguration";
            //
            var e1 = new TfsEndpointOptions();
            e1.SetDefaults();
            e1.Project = "sourceProject";
            Source = e1;
            var e2 = new TfsEndpointOptions();
            e2.SetDefaults();
            e2.Project = "targetProject";
            Target = e2;
        }
    }
}