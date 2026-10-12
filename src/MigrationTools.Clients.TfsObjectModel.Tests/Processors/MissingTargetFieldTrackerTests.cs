using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.Processors.Infrastructure;

namespace MigrationTools.Processors.Tests
{
    [TestClass]
    public class MissingTargetFieldTrackerTests
    {
        [TestMethod, TestCategory("L0")]
        public void Record_ReturnsTrueOnlyForFirstOccurrencePerTypePairAndField()
        {
            var tracker = new MissingTargetFieldTracker();
            Assert.IsTrue(tracker.Record("Bug", "Bug", "Custom.X", "1"));
            Assert.IsFalse(tracker.Record("Bug", "Bug", "Custom.X", "1"));
            Assert.IsFalse(tracker.Record("Bug", "Bug", "Custom.X", "2"));
            Assert.IsTrue(tracker.Record("Bug", "Bug", "Custom.Y", "1"));
            Assert.IsTrue(tracker.Record("Task", "Bug", "Custom.X", "3"));
            Assert.AreEqual(3, tracker.Count);
        }

        [TestMethod, TestCategory("L0")]
        public void GetSummary_ReportsRevisionAndWorkItemCounts()
        {
            var tracker = new MissingTargetFieldTracker();
            tracker.Record("User Story", "Product Backlog Item", "Custom.X", "10");
            tracker.Record("User Story", "Product Backlog Item", "Custom.X", "10");
            tracker.Record("User Story", "Product Backlog Item", "Custom.X", "11");

            var summary = tracker.GetSummary();

            Assert.AreEqual(1, summary.Count);
            StringAssert.Contains(summary[0], "Custom.X");
            StringAssert.Contains(summary[0], "'User Story' -> target type 'Product Backlog Item'");
            StringAssert.Contains(summary[0], "3 revision(s) across 2 work item(s)");
            StringAssert.Contains(summary[0], "[10, 11]");
        }

        [TestMethod, TestCategory("L0")]
        public void GetSummary_TruncatesWorkItemIds()
        {
            var tracker = new MissingTargetFieldTracker();
            for (int i = 0; i < 5; i++)
            {
                tracker.Record("Bug", "Bug", "Custom.X", i.ToString());
            }
            StringAssert.EndsWith(tracker.GetSummary(maxIdsPerField: 2)[0], "[0, 1, ...]");
        }
    }
}
