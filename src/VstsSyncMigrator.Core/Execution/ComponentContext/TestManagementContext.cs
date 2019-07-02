using System.Collections.Generic;
using System.Linq;
using Microsoft.TeamFoundation.TestManagement.Client;

namespace VstsSyncMigrator.Engine.ComponentContext
{
    public class TestManagementContext
    {
        private readonly string testPlanQueryBit;
        private readonly ITeamProjectContext source;
        private ITestManagementService tms;

        internal ITestManagementTeamProject Project { get; }

        public TestManagementContext(ITeamProjectContext source) : this(source, null) { }

        public TestManagementContext(ITeamProjectContext source, string testPlanQueryBit)
        {
            this.testPlanQueryBit = testPlanQueryBit;
            this.source = source;
            tms = (ITestManagementService)source.Collection.GetService(typeof(ITestManagementService));
            Project = tms.GetTeamProject(source.Name);
        }

        internal ITestPlanCollection GetTestPlans()
        {
            var query = (string.IsNullOrWhiteSpace(testPlanQueryBit))
                ? "Select * From TestPlan"
                : $"Select * From TestPlan Where {testPlanQueryBit}";

            return Project.TestPlans.Query(query);
        }

        internal List<ITestRun> GetTestRuns()
        {
            return Project.TestRuns.Query("Select * From TestRun").ToList();
        }

        internal ITestPlan CreateTestPlan()
        {
            return Project.TestPlans.Create();
        }
    }
}