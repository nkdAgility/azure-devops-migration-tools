using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.TestManagement.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;
using System;
using System.Diagnostics;
using VSTS.DataBulkEditor.Engine.ComponentContext;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace VSTS.DataBulkEditor.Engine
{
    //http://stackoverflow.com/questions/6505812/how-to-create-a-test-run-and-result-using-the-team-foundation-server-api
    public class TestRunsMigrationContext : MigrationContextBase
    {

        WorkItemStoreContext sourceWitStore;
        TestManagementContext sourceTestStore;

        WorkItemStoreContext targetWitStore;
        TestManagementContext targetTestStore;

        public override string Name
        {
            get
            {
                return "TestRunsMigrationContext";
            }
        }

        public TestRunsMigrationContext(MigrationEngine me) : base(me)
        {
            sourceWitStore = new WorkItemStoreContext(me.Source, WorkItemStoreFlags.None);
            sourceTestStore = new TestManagementContext(me.Source);
            targetWitStore = new WorkItemStoreContext(me.Target, WorkItemStoreFlags.BypassRules);
            targetTestStore = new TestManagementContext(me.Target);
        }

        internal override void InternalExecute()
        {

            List<ITestRun> sourceRuns = sourceTestStore.GetTestRuns();
            Trace.WriteLine(string.Format("Plan to copy {0} Runs?", sourceRuns.Count), "TestRuns");
            foreach (ITestRun sourceRun in sourceRuns)
            {
                Trace.WriteLine(string.Format("Process Run {0} - ", sourceRun.Id), "TestRuns");
                //ITestRun newRun = targetTestStore.Project.TestRuns.Create();
                throw new NotImplementedException();

            }

        }



    }
}