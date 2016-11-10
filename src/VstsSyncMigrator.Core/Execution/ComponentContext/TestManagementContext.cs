using System;
using System.Collections.Generic;
using Microsoft.TeamFoundation.TestManagement.Client;
using System.Linq;

namespace VstsSyncMigrator.Engine.ComponentContext
{
    public class TestManagementContext
    {
        private ITeamProjectContext source;
        private ITestManagementService tms;
        private ITestManagementTeamProject project;

        internal ITestManagementTeamProject Project { get { return project; } }
  

        public TestManagementContext(ITeamProjectContext source)
        {
            this.source = source;
            tms = (ITestManagementService)source.Collection.GetService(typeof(ITestManagementService));
            project = tms.GetTeamProject(source.Name);
        }

        internal ITestPlanCollection GetTestPlans()
        {
            return project.TestPlans.Query("Select * From TestPlan");
            
        }

        internal List<ITestRun> GetTestRuns()
        {
            return project.TestRuns.Query("Select * From TestRun").ToList();
        }

        internal ITestPlan CreateTestPlan()
        {
            return project.TestPlans.Create();
        }
    }
}