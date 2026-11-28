using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.Services;
using MigrationTools.Tools;

namespace MigrationTools.Tests.Tools
{
    [TestClass]
    public class UnresolvedTestCaseReferenceTests
    {
        private const string M = "src -> tgt";
        private string _logsRoot;

        [TestInitialize]
        public void Setup()
        {
            _logsRoot = Path.Combine(Path.GetTempPath(), "amt-tcref-" + Guid.NewGuid().ToString("N").Substring(0, 8), "logs");
            Directory.CreateDirectory(_logsRoot);
        }

        [TestCleanup]
        public void Cleanup()
        {
            Directory.Delete(Path.GetDirectoryName(_logsRoot), true);
        }

        private WorkItemReviewLog Run(string timestamp)
        {
            var log = new WorkItemReviewLog(Path.Combine(_logsRoot, timestamp));
            log.LoadEarlierRuns(M);
            return log;
        }

        [TestMethod, TestCategory("L0")]
        public void Report_SharedSteps_RecordsOneEntryNamingTheTestCaseAndSourceId()
        {
            using var log = Run("20260101000000");
            using (log.BeginItem("5593", "Test Case"))
            {
                log.MarkChecked(WorkItemReviewCategory.ReferenceLeftOnSource);
                UnresolvedTestCaseReference.Report(null, log, "Shared Steps", "Microsoft.VSTS.TCM.Steps", "5591", "5593", "9344");
            }

            var entry = WorkItemReviewLog.ReadEntries(log.FilePath).Single();
            Assert.AreEqual(WorkItemReviewCategory.ReferenceLeftOnSource, entry.Category);
            Assert.AreEqual("5593", entry.SourceId);
            Assert.AreEqual("9344", entry.TargetId);
            StringAssert.Contains(entry.Reason, "Shared Steps 5591");
            StringAssert.Contains(entry.Reason, "Microsoft.VSTS.TCM.Steps");
            StringAssert.Contains(entry.Action, "Migrate the Shared Steps");
        }

        [TestMethod, TestCategory("L0")]
        public void Report_SharedStepsAndParameter_CountsBothInTheCategory()
        {
            using var log = Run("20260101000000");
            using (log.BeginItem("5593", "Test Case"))
            {
                UnresolvedTestCaseReference.Report(null, log, "Shared Steps", "Microsoft.VSTS.TCM.Steps", "5591", "5593", "9344");
                UnresolvedTestCaseReference.Report(null, log, "Shared Parameter", "Microsoft.VSTS.TCM.LocalDataSource", "5592", "5593", "9344");
            }

            var entries = WorkItemReviewLog.ReadEntries(log.FilePath).Where(e => e.Category == WorkItemReviewCategory.ReferenceLeftOnSource).ToList();
            Assert.AreEqual(2, entries.Count);
            Assert.IsTrue(entries.Any(e => e.Reason.Contains("Shared Parameter 5592")));
        }

        [TestMethod, TestCategory("L0")]
        public void Report_SameReferenceTwiceInOneRun_WritesOneRow()
        {
            using var log = Run("20260101000000");
            using (log.BeginItem("5593", "Test Case"))
            {
                UnresolvedTestCaseReference.Report(null, log, "Shared Steps", "Microsoft.VSTS.TCM.Steps", "5591", "5593", "9344");
                UnresolvedTestCaseReference.Report(null, log, "Shared Steps", "Microsoft.VSTS.TCM.Steps", "5591", "5593", "9344");
            }

            Assert.AreEqual(1, WorkItemReviewLog.ReadEntries(log.FilePath).Count());
        }

        [TestMethod, TestCategory("L0")]
        public void ReRun_ReferenceNowResolved_ResolvesEarlierEntry()
        {
            using (var first = Run("20260101000000"))
            using (first.BeginItem("5593", "Test Case"))
            {
                first.MarkChecked(WorkItemReviewCategory.ReferenceLeftOnSource);
                UnresolvedTestCaseReference.Report(null, first, "Shared Steps", "Microsoft.VSTS.TCM.Steps", "5591", "5593", "9344");
                first.CompleteCurrentItem();
            }

            using var second = Run("20260102000000");
            using (second.BeginItem("5593", "Test Case"))
            {
                // The check ran and found a match, so nothing is reported.
                second.MarkChecked(WorkItemReviewCategory.ReferenceLeftOnSource);
                second.CompleteCurrentItem();
            }

            var resolved = WorkItemReviewLog.ReadEntries(second.FilePath).Where(e => e.Category == WorkItemReviewCategory.Resolved).ToList();
            Assert.AreEqual(1, resolved.Count);
            Assert.AreEqual("5593", resolved[0].SourceId);
        }

        [TestMethod, TestCategory("L0")]
        public void ReRun_LastLinkRemovedFromSource_ResolvesEarlierEntry()
        {
            using (var first = Run("20260101000000"))
            using (first.BeginItem("5593", "Test Case"))
            {
                first.MarkChecked(WorkItemReviewCategory.ReferenceLeftOnSource);
                UnresolvedTestCaseReference.Report(null, first, "Shared Steps", "Microsoft.VSTS.TCM.Steps", "5591", "5593", "9344");
                first.CompleteCurrentItem();
            }

            using var second = Run("20260102000000");
            using (second.BeginItem("5593", "Test Case"))
            {
                // The reference and its last link were removed from the source, so the link tool does not run.
                UnresolvedTestCaseReference.MarkCheckedWithoutLinks(second);
                second.CompleteCurrentItem();
            }

            var resolved = WorkItemReviewLog.ReadEntries(second.FilePath).Where(e => e.Category == WorkItemReviewCategory.Resolved).ToList();
            Assert.AreEqual(1, resolved.Count);
            Assert.AreEqual("5593", resolved[0].SourceId);
        }

        [TestMethod, TestCategory("L0")]
        public void ReRun_CheckNotRun_DoesNotResolveEarlierEntry()
        {
            using (var first = Run("20260101000000"))
            using (first.BeginItem("5593", "Test Case"))
            {
                UnresolvedTestCaseReference.Report(null, first, "Shared Steps", "Microsoft.VSTS.TCM.Steps", "5591", "5593", "9344");
                first.CompleteCurrentItem();
            }

            using var second = Run("20260102000000");
            using (second.BeginItem("5593", "Test Case")) { second.CompleteCurrentItem(); }

            // Nothing resolved, so nothing is written in this run.
            Assert.IsFalse(File.Exists(second.FilePath) && WorkItemReviewLog.ReadEntries(second.FilePath).Any(e => e.Category == WorkItemReviewCategory.Resolved));
        }

        [TestMethod, TestCategory("L0")]
        public void Report_WritesToTheLinksFollowUpFile()
        {
            using var log = Run("20260101000000");
            using (log.BeginItem("5593", "Test Case"))
            {
                UnresolvedTestCaseReference.Report(null, log, "Shared Parameter", "Microsoft.VSTS.TCM.LocalDataSource", "5592", "5593", "9344");
            }

            var followUp = Path.Combine(log.Folder, WorkItemReviewCategory.FollowUpFiles[WorkItemReviewCategory.ReferenceLeftOnSource]);
            Assert.AreEqual(1, WorkItemReviewLog.ReadEntries(followUp).Count());
        }
    }
}
