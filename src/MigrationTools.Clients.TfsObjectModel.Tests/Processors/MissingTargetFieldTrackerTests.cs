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
            Assert.IsTrue(tracker.Record("Bug", "Bug", "Custom.X", "1", 1));
            Assert.IsFalse(tracker.Record("Bug", "Bug", "Custom.X", "1", 2));
            Assert.IsFalse(tracker.Record("Bug", "Bug", "Custom.X", "2", 1));
            Assert.IsTrue(tracker.Record("Bug", "Bug", "Custom.Y", "1", 1));
            Assert.IsTrue(tracker.Record("Task", "Bug", "Custom.X", "3", 1));
            Assert.AreEqual(3, tracker.Count);
        }

        [TestMethod, TestCategory("L0")]
        public void GetSummary_ReportsRevisionAndWorkItemCounts()
        {
            var tracker = new MissingTargetFieldTracker();
            tracker.Record("User Story", "Product Backlog Item", "Custom.X", "10", 1);
            tracker.Record("User Story", "Product Backlog Item", "Custom.X", "10", 2);
            tracker.Record("User Story", "Product Backlog Item", "Custom.X", "11", 1);

            var summary = tracker.GetSummary();

            Assert.AreEqual(1, summary.Count);
            StringAssert.Contains(summary[0], "Custom.X");
            StringAssert.Contains(summary[0], "'User Story' -> target type 'Product Backlog Item'");
            StringAssert.Contains(summary[0], "3 revision(s) across 2 work item(s)");
            StringAssert.Contains(summary[0], "[10, 11]");
        }

        [TestMethod, TestCategory("L0")]
        public void Record_RetriedRevisionIsCountedOnce()
        {
            var tracker = new MissingTargetFieldTracker();
            tracker.Record("Bug", "Bug", "Custom.X", "10", 1);
            tracker.Record("Bug", "Bug", "Custom.X", "10", 2);
            tracker.Record("Bug", "Bug", "Custom.X", "10", 2);

            StringAssert.Contains(tracker.GetSummary()[0], "2 revision(s) across 1 work item(s)");
        }

        [TestMethod, TestCategory("L0")]
        public void GetSummary_TruncatesWorkItemIdsButCountsAll()
        {
            var tracker = new MissingTargetFieldTracker(maxSampleIdsPerField: 2);
            for (int i = 0; i < 5; i++)
            {
                tracker.Record("Bug", "Bug", "Custom.X", i.ToString(), 1);
            }
            var line = tracker.GetSummary()[0];
            StringAssert.Contains(line, "5 revision(s) across 5 work item(s)");
            StringAssert.EndsWith(line, "[0, 1, ...]");
        }
    }
}
