using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace VstsSyncMigrator.Core.Tests
{
    [TestClass]
    public class QueryContextTests
    {
        //[TestMethod]
        //public void TestSimpleQuery()
        //{
        //    TeamProjectContext teamProject = new TeamProjectContext(new System.Uri("https://nkdagility.visualstudio.com"), "vsts-sync-migration");
        //    WorkItemStoreContext sourceStore = new WorkItemStoreContext(teamProject, WorkItemStoreFlags.BypassRules);
        //    TfsQueryContext tfsqc = new TfsQueryContext(sourceStore);
        //    tfsqc.AddParameter("TeamProject", teamProject.Name);
        //    tfsqc.Query = string.Format(@"SELECT [System.Id], [System.Tags] FROM WorkItems WHERE [System.TeamProject] = @TeamProject {0} ORDER BY [System.ChangedDate] desc", "");
        //    WorkItemCollection sourceWIS = tfsqc.Execute();

        //}
    }
}