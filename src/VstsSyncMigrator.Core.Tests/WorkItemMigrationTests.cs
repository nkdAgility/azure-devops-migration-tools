using Microsoft.VisualStudio.TestTools.UnitTesting;
using VstsSyncMigrator.Engine;

namespace VstsSyncMigrator.Core.Tests
{
    [TestClass]
    public class WorkItemMigrationTests
    {        
        [TestMethod]
        public void TestFixAreaPath_WhenNoAreaPathOrNodeBasePaths_DoesntChangeQuery()
        {
            string WIQLQueryBit = @"AND  [Microsoft.VSTS.Common.ClosedDate] = '' AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan')";

            string[] nodeBasePaths = null;
            string targetWIQLQueryBit = WorkItemMigrationContext.FixAreaPathInTargetQuery(WIQLQueryBit, "SourceServer", "TargetServer", nodeBasePaths, null);

            Assert.AreEqual(WIQLQueryBit, targetWIQLQueryBit);
        }

        [TestMethod]
        public void TestFixAreaPath_WhenNoAreaPathAndOneNodeBasePaths_DoesntChangeQuery()
        {
            string WIQLQueryBit = @"AND  [Microsoft.VSTS.Common.ClosedDate] = '' AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan')";

            string[] nodeBasePaths = { "SourceServer\\Area\\Path1" };
            string targetWIQLQueryBit = WorkItemMigrationContext.FixAreaPathInTargetQuery(WIQLQueryBit, "SourceServer", "TargetServer", nodeBasePaths, null);

            Assert.AreEqual(WIQLQueryBit, targetWIQLQueryBit);
        }

        [TestMethod]
        public void TestFixAreaPath_WhenNoAreaPathAndTwoNodeBasePaths_DoesntChangeQuery()
        {
            string WIQLQueryBit = @"AND  [Microsoft.VSTS.Common.ClosedDate] = '' AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan')";

            string[] nodeBasePaths = { "SourceServer\\Area\\Path1", "SourceServer\\Area\\Path1" };
            string targetWIQLQueryBit = WorkItemMigrationContext.FixAreaPathInTargetQuery(WIQLQueryBit, "SourceServer", "TargetServer", nodeBasePaths, null);

            Assert.AreEqual(WIQLQueryBit, targetWIQLQueryBit);
        }


        [TestMethod]
        public void TestFixAreaPath_WhenAreaPathInQueryAndNodeBasePaths_ChangesQuery()
        {
            string WIQLQueryBit =         @"AND [System.AreaPath] = 'SourceServer\Area\Path1' AND   [Microsoft.VSTS.Common.ClosedDate] = '' AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan')";
            string expectTargetQueryBit = @"AND [System.AreaPath] = 'TargetServer\Area\Path1' AND   [Microsoft.VSTS.Common.ClosedDate] = '' AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan')";
            string[] nodeBasePaths = { "SourceServer\\Area\\Path1" };

            string targetWIQLQueryBit = WorkItemMigrationContext.FixAreaPathInTargetQuery(WIQLQueryBit, "SourceServer", "TargetServer", nodeBasePaths, null);

            Assert.AreEqual(expectTargetQueryBit, targetWIQLQueryBit);
        }

        [TestMethod]
        public void TestFixAreaPath_WhenAreaPathInQueryAndTwoNodeBasePaths_ChangesQuery()
        {
            string WIQLQueryBit =         @"AND [System.AreaPath] = 'SourceServer\Area\Path1' OR [System.AreaPath] = 'SourceServer\Area\Path2' AND [Microsoft.VSTS.Common.ClosedDate] = '' AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan')";
            string expectTargetQueryBit = @"AND [System.AreaPath] = 'TargetServer\Area\Path1' OR [System.AreaPath] = 'TargetServer\Area\Path2' AND [Microsoft.VSTS.Common.ClosedDate] = '' AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan')";
            string[] nodeBasePaths = { "SourceServer\\Area\\Path1", "SourceServer\\Area\\Path2" };

            string targetWIQLQueryBit = WorkItemMigrationContext.FixAreaPathInTargetQuery(WIQLQueryBit, "SourceServer", "TargetServer", nodeBasePaths, null);

            Assert.AreEqual(expectTargetQueryBit, targetWIQLQueryBit);
        }

        [TestMethod]
        public void TestFixAreaPath_WhenAreaPathInQueryAndNodeBasePathsIsMoreSpecific_DoesntChangeQuery()
        {
            string WIQLQueryBit = @"AND [System.AreaPath] = 'SourceServer\Area\' AND [Microsoft.VSTS.Common.ClosedDate] = '' AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan')";            
            string[] nodeBasePaths = { "SourceServer\\Area\\Path1", "SourceServer\\Area\\Path2" };

            string targetWIQLQueryBit = WorkItemMigrationContext.FixAreaPathInTargetQuery(WIQLQueryBit, "SourceServer", "TargetServer", nodeBasePaths, null);

            Assert.AreEqual(WIQLQueryBit, targetWIQLQueryBit);
        }

        [TestMethod]
        public void TestFixAreaPath_WhenAreaPathAtEndOfQueryAndNodeBasePaths_ChangesQuery()
        {
            string WIQLQueryBit =         @"AND [Microsoft.VSTS.Common.ClosedDate] = '' AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan') AND [System.AreaPath] = 'SourceServer\Area\Path1'";
            string expectTargetQueryBit = @"AND [Microsoft.VSTS.Common.ClosedDate] = '' AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan') AND [System.AreaPath] = 'TargetServer\Area\Path1'";
            string[] nodeBasePaths = { "SourceServer\\Area\\Path1" };

            string targetWIQLQueryBit = WorkItemMigrationContext.FixAreaPathInTargetQuery(WIQLQueryBit, "SourceServer", "TargetServer", nodeBasePaths, null);

            Assert.AreEqual(expectTargetQueryBit, targetWIQLQueryBit);
        }
    }
}