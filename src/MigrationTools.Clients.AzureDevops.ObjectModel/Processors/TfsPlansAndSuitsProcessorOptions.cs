using System;
using MigrationTools.Endpoints;

namespace MigrationTools.Processors
{
    /// <summary>
    /// The `TfsPlansAndSuitsProcessor` enabled you to migrate queries from one locatio nto another.
    /// </summary>
    public class TfsPlansAndSuitsProcessorOptions : ProcessorOptions
    {
        public bool FilterCompleted { get; set; }
        public string OnlyElementsWithTag { get; set; }
        public bool PrefixProjectToNodes { get; set; }

        /// <summary>
        /// Remove Invalid Links, see https://github.com/nkdAgility/azure-devops-migration-tools/issues/178
        /// </summary>
        public bool RemoveInvalidTestSuiteLinks { get; set; }

        public string TestPlanQueryBit { get; set; }

        public override Type ToConfigure => typeof(TfsSharedQueryProcessor);

        public override string Processor => ToConfigure.Name;

        public override IProcessorOptions GetDefault()
        {
            return this;
        }

        public override void SetDefaults()
        {
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