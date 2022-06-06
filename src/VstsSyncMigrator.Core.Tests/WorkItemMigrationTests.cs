using Microsoft.VisualStudio.TestTools.UnitTesting;
using VstsSyncMigrator.Engine;

namespace VstsSyncMigrator.Core.Tests
{
    [TestClass]
    public class WorkItemMigrationTests
    {
        [TestMethod]
        public void TestFixAreaPath_WhenNoAreaPathOrIterationPath_DoesntChangeQuery()
        {
            string WIQLQueryBit = @"AND  [Microsoft.VSTS.Common.ClosedDate] = '' AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan')";

            string targetWIQLQueryBit = WorkItemMigrationContext.FixAreaPathAndIterationPathForTargetQuery(WIQLQueryBit, "SourceServer", "TargetServer", false, null);

            Assert.AreEqual(WIQLQueryBit, targetWIQLQueryBit);
        }


        [TestMethod]
        public void TestFixAreaPath_WhenAreaPathInQuery_ChangesQuery()
        {
            string WIQLQueryBit = @"AND [System.AreaPath] = 'SourceServer\Area\Path1' AND   [Microsoft.VSTS.Common.ClosedDate] = '' AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan')";
            string expectTargetQueryBit = @"AND [System.AreaPath] = 'TargetServer\Area\Path1' AND   [Microsoft.VSTS.Common.ClosedDate] = '' AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan')";

            string targetWIQLQueryBit = WorkItemMigrationContext.FixAreaPathAndIterationPathForTargetQuery(WIQLQueryBit, "SourceServer", "TargetServer", false, null);

            Assert.AreEqual(expectTargetQueryBit, targetWIQLQueryBit);
        }

        [TestMethod]
        public void TestFixAreaPath_WhenAreaPathInQuery_WithPrefixProjectToNodesEnabled_ChangesQuery()
        {
            string WIQLQueryBit = @"AND [System.AreaPath] = 'SourceServer\Area\Path1' AND   [Microsoft.VSTS.Common.ClosedDate] = '' AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan')";
            string expectTargetQueryBit = @"AND [System.AreaPath] = 'TargetServer\SourceServer\Area\Path1' AND   [Microsoft.VSTS.Common.ClosedDate] = '' AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan')";

            string targetWIQLQueryBit = WorkItemMigrationContext.FixAreaPathAndIterationPathForTargetQuery(WIQLQueryBit, "SourceServer", "TargetServer", true, null);

            Assert.AreEqual(expectTargetQueryBit, targetWIQLQueryBit);
        }

        [TestMethod]
        public void TestFixAreaPath_WhenMultipleAreaPathInQuery_ChangesQuery()
        {
            string WIQLQueryBit = @"AND [System.AreaPath] = 'SourceServer\Area\Path1' OR [System.AreaPath] = 'SourceServer\Area\Path2' AND [Microsoft.VSTS.Common.ClosedDate] = '' AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan')";
            string expectTargetQueryBit = @"AND [System.AreaPath] = 'TargetServer\Area\Path1' OR [System.AreaPath] = 'TargetServer\Area\Path2' AND [Microsoft.VSTS.Common.ClosedDate] = '' AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan')";

            string targetWIQLQueryBit = WorkItemMigrationContext.FixAreaPathAndIterationPathForTargetQuery(WIQLQueryBit, "SourceServer", "TargetServer", false, null);

            Assert.AreEqual(expectTargetQueryBit, targetWIQLQueryBit);
        }

        [TestMethod]
        public void TestFixAreaPath_WhenAreaPathAtEndOfQuery_ChangesQuery()
        {
            string WIQLQueryBit = @"AND [Microsoft.VSTS.Common.ClosedDate] = '' AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan') AND [System.AreaPath] = 'SourceServer\Area\Path1'";
            string expectTargetQueryBit = @"AND [Microsoft.VSTS.Common.ClosedDate] = '' AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan') AND [System.AreaPath] = 'TargetServer\Area\Path1'";

            string targetWIQLQueryBit = WorkItemMigrationContext.FixAreaPathAndIterationPathForTargetQuery(WIQLQueryBit, "SourceServer", "TargetServer", false, null);

            Assert.AreEqual(expectTargetQueryBit, targetWIQLQueryBit);
        }

        [TestMethod]
        public void TestFixIterationPath_WhenInQuery_ChangesQuery()
        {
            string WIQLQueryBit = @"AND [System.IterationPath] = 'SourceServer\Iteration\Path1' AND   [Microsoft.VSTS.Common.ClosedDate] = '' AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan')";
            string expectTargetQueryBit = @"AND [System.IterationPath] = 'TargetServer\Iteration\Path1' AND   [Microsoft.VSTS.Common.ClosedDate] = '' AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan')";

            string targetWIQLQueryBit = WorkItemMigrationContext.FixAreaPathAndIterationPathForTargetQuery(WIQLQueryBit, "SourceServer", "TargetServer", false, null);

            Assert.AreEqual(expectTargetQueryBit, targetWIQLQueryBit);
        }

        [TestMethod]
        public void TestFixAreaPathAndIteration_WhenMultipleOccuranceInQuery_ChangesQuery()
        {
            string WIQLQueryBit = @"AND ([System.AreaPath] = 'SourceServer\Area\Path1' OR [System.AreaPath] = 'SourceServer\Area\Path2') AND ([System.IterationPath] = 'SourceServer\Iteration\Path1' OR [System.IterationPath] = 'SourceServer\Iteration\Path2') AND [Microsoft.VSTS.Common.ClosedDate] = '' AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan')";
            string expectTargetQueryBit = @"AND ([System.AreaPath] = 'TargetServer\Area\Path1' OR [System.AreaPath] = 'TargetServer\Area\Path2') AND ([System.IterationPath] = 'TargetServer\Iteration\Path1' OR [System.IterationPath] = 'TargetServer\Iteration\Path2') AND [Microsoft.VSTS.Common.ClosedDate] = '' AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan')";

            string targetWIQLQueryBit = WorkItemMigrationContext.FixAreaPathAndIterationPathForTargetQuery(WIQLQueryBit, "SourceServer", "TargetServer", false, null);

            Assert.AreEqual(expectTargetQueryBit, targetWIQLQueryBit);
        }

        [TestMethod]
        public void TestFixAreaPathAndIteration_WhenMultipleOccuranceWithMixtureOrEqualAndUnderOperatorsInQuery_ChangesQuery()
        {
            string WIQLQueryBit = @"AND ([System.AreaPath] = 'SourceServer\Area\Path1' OR [System.AreaPath] UNDER 'SourceServer\Area\Path2') AND ([System.IterationPath] UNDER 'SourceServer\Iteration\Path1' OR [System.IterationPath] = 'SourceServer\Iteration\Path2') AND [Microsoft.VSTS.Common.ClosedDate] = '' AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan')";
            string expectTargetQueryBit = @"AND ([System.AreaPath] = 'TargetServer\Area\Path1' OR [System.AreaPath] UNDER 'TargetServer\Area\Path2') AND ([System.IterationPath] UNDER 'TargetServer\Iteration\Path1' OR [System.IterationPath] = 'TargetServer\Iteration\Path2') AND [Microsoft.VSTS.Common.ClosedDate] = '' AND [System.WorkItemType] NOT IN ('Test Suite', 'Test Plan')";

            string targetWIQLQueryBit = WorkItemMigrationContext.FixAreaPathAndIterationPathForTargetQuery(WIQLQueryBit, "SourceServer", "TargetServer", false, null);

            Assert.AreEqual(expectTargetQueryBit, targetWIQLQueryBit);
        }
    }
}