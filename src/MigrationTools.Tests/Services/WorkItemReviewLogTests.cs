using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.Services;

namespace MigrationTools.Tests.Services
{
    [TestClass]
    public class WorkItemReviewLogTests
    {
        private string _logsRoot;

        [TestInitialize]
        public void Setup()
        {
            _logsRoot = Path.Combine(Path.GetTempPath(), "amt-reviewlog-" + Guid.NewGuid().ToString("N").Substring(0, 8), "logs");
            Directory.CreateDirectory(_logsRoot);
        }

        [TestCleanup]
        public void Cleanup()
        {
            Directory.Delete(Path.GetDirectoryName(_logsRoot), true);
        }

        private const string M = "src -> tgt";

        private static WorkItemReviewLog Earlier(string folder)
        {
            var log = new WorkItemReviewLog(folder);
            log.LoadEarlierRuns(M);
            return log;
        }

        private string RunFolder(string timestamp) => Path.Combine(_logsRoot, timestamp);

        [TestMethod, TestCategory("L0")]
        public void Record_WritesEachEntryToDiskBeforeDispose()
        {
            var log = Earlier(RunFolder("20260101000000"));
            using (log.BeginItem("12", "Bug", 3))
            {
                log.SetCurrentItem(targetId: "99");
                log.Record(WorkItemReviewCategory.Failed, "boom", "fix it");
            }

            // Read while the stream is still open, as after a kill the process never disposes it.
            var entries = WorkItemReviewLog.ReadEntries(log.FilePath).ToList();
            Assert.AreEqual(1, entries.Count);
            Assert.AreEqual("12", entries[0].SourceId);
            Assert.AreEqual("99", entries[0].TargetId);
            Assert.AreEqual("Bug", entries[0].WorkItemType);
            Assert.AreEqual(3, entries[0].Revision);
            Assert.AreEqual("failed", entries[0].Category);
            Assert.AreEqual("boom", entries[0].Reason);
            Assert.AreEqual("fix it", entries[0].Action);
            log.Dispose();
        }

        [TestMethod, TestCategory("L0")]
        public void Record_NoEntries_CreatesNoFiles()
        {
            var folder = RunFolder("20260101000000");
            using var log = new WorkItemReviewLog(folder);
            using (log.BeginItem("1", "Task"))
            {
                log.CompleteCurrentItem();
            }
            Assert.IsFalse(Directory.Exists(folder) && Directory.GetFiles(folder).Any());
        }

        [TestMethod, TestCategory("L0")]
        public void Record_SameProblemTwice_WritesOnce()
        {
            using var log = Earlier(RunFolder("20260101000000"));
            using (log.BeginItem("5", "Bug"))
            {
                log.Record(WorkItemReviewCategory.FieldNotMapped, "field X missing", "add it", revision: 1);
                log.Record(WorkItemReviewCategory.FieldNotMapped, "field X missing", "add it", revision: 2);
            }
            Assert.AreEqual(1, WorkItemReviewLog.ReadEntries(log.FilePath).Count());
            Assert.AreEqual(1, log.CategoryCounts[WorkItemReviewCategory.FieldNotMapped]);
        }

        [TestMethod, TestCategory("L0")]
        public void Record_FollowUpCategory_AlsoWritesFollowUpFile()
        {
            using var log = Earlier(RunFolder("20260101000000"));
            log.Record(WorkItemReviewCategory.UserNotMapped, "user", "map", sourceId: "1");
            log.Record(WorkItemReviewCategory.Failed, "boom", "fix", sourceId: "2");

            var followUp = Path.Combine(log.Folder, "follow-up-users-not-mapped.jsonl");
            Assert.AreEqual(1, WorkItemReviewLog.ReadEntries(followUp).Count());
            Assert.AreEqual(2, WorkItemReviewLog.ReadEntries(log.FilePath).Count());
            Assert.AreEqual(2, Directory.GetFiles(log.Folder).Length, "failed has no follow-up file");
        }

        [TestMethod, TestCategory("L0")]
        public void ReadEntries_TruncatedLastLine_IsSkipped()
        {
            var folder = RunFolder("20260101000000");
            using (var log = new WorkItemReviewLog(folder))
            {
                log.Record(WorkItemReviewCategory.Failed, "boom", "fix", sourceId: "1");
            }
            File.AppendAllText(Path.Combine(folder, WorkItemReviewLog.FileName), "{\"time\":\"2026-01-01T00:00");
            Assert.AreEqual(1, WorkItemReviewLog.ReadEntries(Path.Combine(folder, WorkItemReviewLog.FileName)).Count());
        }

        [TestMethod, TestCategory("L0")]
        public void CompleteCurrentItem_ItemOpenInEarlierRun_WritesResolved()
        {
            using (var first = Earlier(RunFolder("20260101000000")))
            {
                first.Record(WorkItemReviewCategory.PartMigrated, "boom", "fix", sourceId: "7", revision: 1);
                first.Record(WorkItemReviewCategory.Failed, "other", "fix", sourceId: "8");
            }

            using var second = Earlier(RunFolder("20260102000000"));
            second.LoadEarlierRuns(M);
            using (second.BeginItem("7", "Bug"))
            {
                second.SetCurrentItem(targetId: "70");
                second.MarkRevisionReplayed(1);
                second.CompleteCurrentItem();
            }
            using (second.BeginItem("8", "Bug"))
            {
                second.Record(WorkItemReviewCategory.Failed, "other", "fix");
                second.CompleteCurrentItem();
            }

            var entries = WorkItemReviewLog.ReadEntries(second.FilePath).ToList();
            var resolved = entries.Single(e => e.Category == WorkItemReviewCategory.Resolved);
            Assert.AreEqual("7", resolved.SourceId);
            Assert.AreEqual("70", resolved.TargetId);
            StringAssert.Contains(resolved.Reason, "part-migrated");
            Assert.IsFalse(entries.Any(e => e.SourceId == "8" && e.Category == WorkItemReviewCategory.Resolved));
        }

        [TestMethod, TestCategory("L0")]
        public void CompleteCurrentItem_NoRevisionReplayed_DoesNotResolveReplayOnlyProblem()
        {
            using (var first = Earlier(RunFolder("20260101000000")))
            {
                first.Record(WorkItemReviewCategory.FieldNotMapped, "field X", "add it", sourceId: "7");
                first.Record(WorkItemReviewCategory.LinkSkipped, "link", "migrate it", sourceId: "8");
            }
            using var second = Earlier(RunFolder("20260102000000"));
            second.LoadEarlierRuns(M);
            using (second.BeginItem("7", "Task")) { second.CompleteCurrentItem(); }
            using (second.BeginItem("8", "Task")) { second.MarkChecked(WorkItemReviewCategory.LinkSkipped); second.CompleteCurrentItem(); }

            var resolved = WorkItemReviewLog.ReadEntries(second.FilePath).Where(e => e.Category == WorkItemReviewCategory.Resolved).ToList();
            Assert.AreEqual(1, resolved.Count, "only the link problem is checked again without a replayed revision");
            Assert.AreEqual("8", resolved[0].SourceId);
            var latest = WorkItemReviewLog.ReadEntries(second.WriteLatestView()).ToList();
            Assert.AreEqual(WorkItemReviewCategory.FieldNotMapped, latest.Single(e => e.SourceId == "7").Category);
        }

        [TestMethod, TestCategory("L0")]
        public void CompleteCurrentItem_AlreadyResolved_DoesNotResolveAgain()
        {
            using (var first = Earlier(RunFolder("20260101000000")))
            {
                first.Record(WorkItemReviewCategory.Failed, "boom", "fix", sourceId: "7", revision: 1);
            }
            using (var second = Earlier(RunFolder("20260102000000")))
            {
                second.LoadEarlierRuns(M);
                using (second.BeginItem("7", "Bug")) { second.MarkRevisionReplayed(1); second.CompleteCurrentItem(); }
                Assert.IsTrue(File.Exists(second.FilePath));
            }
            using var third = Earlier(RunFolder("20260103000000"));
            third.LoadEarlierRuns(M);
            using (third.BeginItem("7", "Bug")) { third.MarkRevisionReplayed(1); third.CompleteCurrentItem(); }
            Assert.IsFalse(File.Exists(third.FilePath));
        }

        [TestMethod, TestCategory("L0")]
        public void LoadEarlierRuns_IgnoresLaterRuns()
        {
            using (var later = Earlier(RunFolder("20260105000000")))
            {
                later.Record(WorkItemReviewCategory.Failed, "boom", "fix", sourceId: "7", revision: 1);
            }
            using var run = Earlier(RunFolder("20260102000000"));
            run.LoadEarlierRuns(M);
            using (run.BeginItem("7", "Bug")) { run.MarkRevisionReplayed(1); run.CompleteCurrentItem(); }
            Assert.IsFalse(File.Exists(run.FilePath));
        }

        [TestMethod, TestCategory("L0")]
        public void WriteLatestView_KeepsLatestEntryPerItem()
        {
            using (var first = Earlier(RunFolder("20260101000000")))
            {
                first.Record(WorkItemReviewCategory.Failed, "boom", "fix", sourceId: "7", revision: 1);
                first.Record(WorkItemReviewCategory.Failed, "boom", "fix", sourceId: "9");
            }
            using var second = Earlier(RunFolder("20260102000000"));
            second.LoadEarlierRuns(M);
            using (second.BeginItem("7", "Bug")) { second.MarkRevisionReplayed(1); second.CompleteCurrentItem(); }
            var path = second.WriteLatestView();

            var latest = WorkItemReviewLog.ReadEntries(path).ToList();
            Assert.AreEqual(2, latest.Count);
            Assert.AreEqual(WorkItemReviewCategory.Resolved, latest.Single(e => e.SourceId == "7").Category);
            Assert.AreEqual(WorkItemReviewCategory.Failed, latest.Single(e => e.SourceId == "9").Category);
        }

        [TestMethod, TestCategory("L0")]
        public void CurrentItemHas_TracksCategoriesOfCurrentItemOnly()
        {
            using var log = Earlier(RunFolder("20260101000000"));
            using (log.BeginItem("1", "Bug"))
            {
                log.Record(WorkItemReviewCategory.RerunMismatch, "x", "y");
                Assert.IsTrue(log.CurrentItemHas(WorkItemReviewCategory.RerunMismatch));
            }
            using (log.BeginItem("2", "Bug"))
            {
                Assert.IsFalse(log.CurrentItemHas(WorkItemReviewCategory.RerunMismatch));
            }
        }

        [TestMethod, TestCategory("L0")]
        public void LoadEarlierRuns_OtherMigration_IsIgnored()
        {
            using (var other = new WorkItemReviewLog(RunFolder("20260101000000")))
            {
                other.LoadEarlierRuns("other-src -> other-tgt");
                other.Record(WorkItemReviewCategory.Failed, "boom", "fix", sourceId: "7", revision: 1);
            }
            using var run = Earlier(RunFolder("20260102000000"));
            using (run.BeginItem("7", "Bug")) { run.MarkRevisionReplayed(1); run.CompleteCurrentItem(); }
            Assert.IsFalse(File.Exists(run.FilePath));
            Assert.IsNull(run.WriteLatestView());
        }

        [TestMethod, TestCategory("L0")]
        public void CompleteCurrentItem_OtherRevisionReplayed_DoesNotResolve()
        {
            using (var first = Earlier(RunFolder("20260101000000")))
            {
                first.Record(WorkItemReviewCategory.FieldNotMapped, "field X", "add it", sourceId: "7", revision: 2);
            }
            using var second = Earlier(RunFolder("20260102000000"));
            using (second.BeginItem("7", "Task")) { second.MarkRevisionReplayed(5); second.CompleteCurrentItem(); }
            Assert.IsFalse(File.Exists(second.FilePath), "revision 2, where the field was dropped, was not replayed");
        }

        [TestMethod, TestCategory("L0")]
        public void Record_CarriesTheMigration()
        {
            using var log = Earlier(RunFolder("20260101000000"));
            log.Record(WorkItemReviewCategory.Failed, "boom", "fix", sourceId: "1");
            Assert.AreEqual(M, WorkItemReviewLog.ReadEntries(log.FilePath).Single().Migration);
        }

        [TestMethod, TestCategory("L0")]
        public void IsMigratedLater_TrueUntilTheItemIsStarted()
        {
            using var log = Earlier(RunFolder("20260101000000"));
            log.SetItemsToMigrate(new[] { "1", "2" });
            Assert.IsTrue(log.IsMigratedLater("2"));
            using (log.BeginItem("2", "Task")) { }
            Assert.IsFalse(log.IsMigratedLater("2"));
            Assert.IsTrue(log.IsMigratedLater("1"));
            Assert.IsFalse(log.IsMigratedLater("3"));
        }

        [TestMethod, TestCategory("L0")]
        public void CompleteCurrentItem_CheckDidNotRun_DoesNotResolve()
        {
            using (var first = Earlier(RunFolder("20260101000000")))
            {
                first.Record(WorkItemReviewCategory.AttachmentNotMigrated, "too big", "re-upload", sourceId: "7");
            }
            using var second = Earlier(RunFolder("20260102000000"));
            using (second.BeginItem("7", "Task")) { second.CompleteCurrentItem(); }
            Assert.IsFalse(File.Exists(second.FilePath), "the attachment tool did not run, so the problem was not checked again");
            using (second.BeginItem("7", "Task")) { second.MarkChecked(WorkItemReviewCategory.AttachmentNotMigrated); second.CompleteCurrentItem(); }
            Assert.AreEqual(WorkItemReviewCategory.Resolved, WorkItemReviewLog.ReadEntries(second.FilePath).Single().Category);
        }

        [TestMethod, TestCategory("L0")]
        public void CompleteCurrentItem_PathFromValidation_ResolvesOnlyAfterValidationPassed()
        {
            using (var first = Earlier(RunFolder("20260101000000")))
            {
                first.Record(WorkItemReviewCategory.PathNotMapped, "missing path", "map it", sourceId: "7");
            }
            using var second = Earlier(RunFolder("20260102000000"));
            using (second.BeginItem("7", "Task")) { second.CompleteCurrentItem(); }
            Assert.IsFalse(File.Exists(second.FilePath), "path validation did not run (TfsNodeStructureTool disabled)");
            second.MarkPathsValidated();
            using (second.BeginItem("7", "Task")) { second.CompleteCurrentItem(); }
            Assert.AreEqual(WorkItemReviewCategory.Resolved, WorkItemReviewLog.ReadEntries(second.FilePath).Single().Category);
        }

        [TestMethod, TestCategory("L0")]
        public void LoadEarlierRuns_SecondMigrationInTheSameRun_ReadsItsOwnHistory()
        {
            using (var first = new WorkItemReviewLog(RunFolder("20260101000000")))
            {
                first.LoadEarlierRuns("a -> b");
                first.Record(WorkItemReviewCategory.LinkSkipped, "link", "migrate", sourceId: "7");
            }
            using var run = new WorkItemReviewLog(RunFolder("20260102000000"));
            run.LoadEarlierRuns("x -> y");
            using (run.BeginItem("7", "Task")) { run.MarkChecked(WorkItemReviewCategory.LinkSkipped); run.CompleteCurrentItem(); }
            Assert.IsFalse(File.Exists(run.FilePath), "item 7 of another migration must not be resolved");
            run.LoadEarlierRuns("a -> b");
            using (run.BeginItem("7", "Task")) { run.MarkChecked(WorkItemReviewCategory.LinkSkipped); run.CompleteCurrentItem(); }
            var resolved = WorkItemReviewLog.ReadEntries(run.FilePath).Single();
            Assert.AreEqual(WorkItemReviewCategory.Resolved, resolved.Category);
            Assert.AreEqual("a -> b", resolved.Migration);
        }

        [TestMethod, TestCategory("L0")]
        public void LoadEarlierRuns_UnreadableFile_DoesNotThrow()
        {
            using (var first = Earlier(RunFolder("20260101000000")))
            {
                first.Record(WorkItemReviewCategory.Failed, "boom", "fix", sourceId: "7", revision: 1);
            }
            using (new FileStream(Path.Combine(RunFolder("20260101000000"), WorkItemReviewLog.FileName), FileMode.Open, FileAccess.Read, FileShare.None))
            {
                using var run = Earlier(RunFolder("20260102000000"));
                using (run.BeginItem("7", "Bug")) { run.MarkRevisionReplayed(1); run.CompleteCurrentItem(); }
                Assert.IsFalse(File.Exists(run.FilePath));
            }
        }

        [TestMethod, TestCategory("L0")]
        public void CompleteCurrentItem_OneEarlierLogUnreadable_DoesNotResolve()
        {
            using (var first = Earlier(RunFolder("20260101000000")))
            {
                first.Record(WorkItemReviewCategory.Failed, "boom", "fix", sourceId: "7", revision: 1);
            }
            using (var second = Earlier(RunFolder("20260102000000")))
            {
                second.Record(WorkItemReviewCategory.FieldNotMapped, "field X", "add it", sourceId: "7", revision: 2);
            }
            using (new FileStream(Path.Combine(RunFolder("20260102000000"), WorkItemReviewLog.FileName), FileMode.Open, FileAccess.Read, FileShare.None))
            {
                using var run = Earlier(RunFolder("20260103000000"));
                using (run.BeginItem("7", "Bug")) { run.MarkRevisionReplayed(1); run.CompleteCurrentItem(); }
                Assert.IsFalse(File.Exists(run.FilePath), "the unreadable log may hold a problem on a revision that was not replayed");
            }
        }

        [TestMethod, TestCategory("L0")]
        public void CompleteCurrentItem_EarlierLogAccessDenied_DoesNotResolve()
        {
            using (var first = Earlier(RunFolder("20260101000000")))
            {
                first.Record(WorkItemReviewCategory.Failed, "boom", "fix", sourceId: "7", revision: 1);
            }
            // A directory where the log should be cannot be opened as a file, as when access to the file is denied.
            Directory.CreateDirectory(Path.Combine(RunFolder("20260102000000"), WorkItemReviewLog.FileName));
            using var run = Earlier(RunFolder("20260103000000"));
            using (run.BeginItem("7", "Bug")) { run.MarkRevisionReplayed(1); run.CompleteCurrentItem(); }
            Assert.IsFalse(File.Exists(run.FilePath));
        }

        [TestMethod, TestCategory("L0")]
        public void CompleteCurrentItem_EarlierRunWithoutReviewLog_StillResolves()
        {
            using (var first = Earlier(RunFolder("20260101000000")))
            {
                first.Record(WorkItemReviewCategory.Failed, "boom", "fix", sourceId: "7", revision: 1);
            }
            Directory.CreateDirectory(RunFolder("20260102000000"));
            using var run = Earlier(RunFolder("20260103000000"));
            using (run.BeginItem("7", "Bug")) { run.MarkRevisionReplayed(1); run.CompleteCurrentItem(); }
            Assert.AreEqual(WorkItemReviewCategory.Resolved, WorkItemReviewLog.ReadEntries(run.FilePath).Single().Category);
        }

        [TestMethod, TestCategory("L0")]
        public void Record_FlushFails_RetryLeavesValidLinesAndCountsOnce()
        {
            var folder = RunFolder("20260101000000");
            var opened = 0;
            using var log = new WorkItemReviewLog(folder, null, path =>
            {
                opened++;
                var file = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                return opened == 1 ? new FailingFlushStream(file) : file;
            });
            log.Record(WorkItemReviewCategory.Failed, "boom", "fix", sourceId: "7");
            Assert.AreEqual(0, log.CategoryCounts.Count, "a row whose flush failed is not counted");
            log.Record(WorkItemReviewCategory.Failed, "boom", "fix", sourceId: "7");
            log.Record(WorkItemReviewCategory.Failed, "boom", "fix", sourceId: "7");

            string text; using (var reader = new StreamReader(new FileStream(log.FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))) { text = reader.ReadToEnd(); }
            var lines = text.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            Assert.IsTrue(lines.Length >= 1 && lines.Length <= 2, $"expected the row once, or twice after the failed flush; got {lines.Length}");
            foreach (var line in lines)
            {
                Assert.AreEqual("7", Newtonsoft.Json.JsonConvert.DeserializeObject<WorkItemReviewEntry>(line).SourceId);
            }
            Assert.AreEqual(1, log.CategoryCounts[WorkItemReviewCategory.Failed]);
            Assert.AreEqual(1, log.FileRowCounts[WorkItemReviewLog.FileName]);
        }

        private sealed class FailingFlushStream : Stream
        {
            private readonly Stream _inner;
            public FailingFlushStream(Stream inner) { _inner = inner; }
            public override bool CanRead => false;
            public override bool CanSeek => false;
            public override bool CanWrite => true;
            public override long Length => _inner.Length;
            public override long Position { get => _inner.Position; set => throw new NotSupportedException(); }
            public override void Flush() => throw new IOException("disk full");
            public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
            public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
            public override void SetLength(long value) => throw new NotSupportedException();
            public override void Write(byte[] buffer, int offset, int count) => _inner.Write(buffer, offset, count);
            protected override void Dispose(bool disposing) { if (disposing) _inner.Dispose(); base.Dispose(disposing); }
        }

        [TestMethod, TestCategory("L0")]
        public void RecordUnlessMigratedLater_RelatedItemMigrates_EntryIsDropped()
        {
            using var log = Earlier(RunFolder("20260101000000"));
            log.SetItemsToMigrate(new[] { "1", "2" });
            using (log.BeginItem("1", "Task")) { log.RecordUnlessMigratedLater("2", WorkItemReviewCategory.LinkSkipped, "link to 2", "migrate 2"); }
            using (log.BeginItem("2", "Task")) { log.CompleteCurrentItem(); }
            log.FlushHeldEntries();
            Assert.IsFalse(File.Exists(log.FilePath));
        }

        [TestMethod, TestCategory("L0")]
        public void RecordUnlessMigratedLater_RelatedItemFails_EntryIsWrittenForTheWaitingItem()
        {
            using var log = Earlier(RunFolder("20260101000000"));
            log.SetItemsToMigrate(new[] { "1", "2" });
            using (log.BeginItem("1", "Task")) { log.SetCurrentItem(targetId: "10"); log.RecordUnlessMigratedLater("2", WorkItemReviewCategory.LinkSkipped, "link to 2.", "migrate 2"); }
            using (log.BeginItem("2", "Bug")) { log.ItemNotMigrated("2"); log.Record(WorkItemReviewCategory.Failed, "boom", "fix"); }
            var entries = WorkItemReviewLog.ReadEntries(log.FilePath).ToList();
            var link = entries.Single(e => e.Category == WorkItemReviewCategory.LinkSkipped);
            Assert.AreEqual("1", link.SourceId);
            Assert.AreEqual("10", link.TargetId);
            StringAssert.Contains(link.Reason, "2 was not migrated");
        }

        [TestMethod, TestCategory("L0")]
        public void CompleteCurrentItem_UpdateFailed_ResolvesWithoutReplay()
        {
            using (var first = Earlier(RunFolder("20260101000000")))
            {
                first.Record(WorkItemReviewCategory.UpdateFailed, "attachments failed", "re-run", sourceId: "7", revision: 4);
            }
            using var run = Earlier(RunFolder("20260102000000"));
            using (run.BeginItem("7", "Task")) { run.CompleteCurrentItem(); }
            Assert.AreEqual(WorkItemReviewCategory.Resolved, WorkItemReviewLog.ReadEntries(run.FilePath).Single().Category);
        }

        [TestMethod, TestCategory("L0")]
        public void CurrentItemReplayedRevisions_TrueOnlyAfterAReplay()
        {
            using var log = Earlier(RunFolder("20260101000000"));
            using (log.BeginItem("1", "Task"))
            {
                Assert.IsFalse(log.CurrentItemReplayedRevisions);
                log.MarkRevisionReplayed(2);
                Assert.IsTrue(log.CurrentItemReplayedRevisions);
            }
        }
        [TestMethod, TestCategory("L0")]
        public void CompleteCurrentItem_LastLinkPassSkipped_DoesNotResolve()
        {
            using (var first = Earlier(RunFolder("20260101000000")))
            {
                first.Record(WorkItemReviewCategory.LinkSkipped, "link", "migrate", sourceId: "7");
            }
            using var run = Earlier(RunFolder("20260102000000"));
            using (run.BeginItem("7", "Task"))
            {
                run.MarkChecked(WorkItemReviewCategory.LinkSkipped);
                run.ClearChecked(WorkItemReviewCategory.LinkSkipped);
                run.CompleteCurrentItem();
            }
            Assert.IsFalse(File.Exists(run.FilePath), "the final revision's links were skipped by the count filter");
        }

        [TestMethod, TestCategory("L0")]
        public void CompleteCurrentItem_EmbeddedImagesNotChecked_DoesNotResolveImageProblem()
        {
            using (var first = Earlier(RunFolder("20260101000000")))
            {
                first.Record(WorkItemReviewCategory.EmbeddedImageNotMigrated, "image", "re-upload", sourceId: "7");
            }
            using var run = Earlier(RunFolder("20260102000000"));
            using (run.BeginItem("7", "Task")) { run.MarkChecked(WorkItemReviewCategory.AttachmentNotMigrated); run.CompleteCurrentItem(); }
            Assert.IsFalse(File.Exists(run.FilePath), "FixHtmlAttachmentLinks was off, so the image was not checked");
            using (run.BeginItem("7", "Task")) { run.MarkChecked(WorkItemReviewCategory.EmbeddedImageNotMigrated); run.CompleteCurrentItem(); }
            Assert.AreEqual(WorkItemReviewCategory.Resolved, WorkItemReviewLog.ReadEntries(run.FilePath).Single().Category);
        }
        [TestMethod, TestCategory("L0")]
        public void CompleteCurrentItem_EarlierLogHasBrokenMiddleLine_DoesNotResolve()
        {
            using (var first = Earlier(RunFolder("20260101000000")))
            {
                first.Record(WorkItemReviewCategory.LinkSkipped, "link", "migrate", sourceId: "7");
            }
            var file = Path.Combine(RunFolder("20260101000000"), WorkItemReviewLog.FileName);
            var text = File.ReadAllText(file);
            File.WriteAllText(file, "{\"sourceId\":\"7\",\"categ\n" + text);
            WorkItemReviewLog.ReadEntries(file, out var skipped);
            Assert.IsTrue(skipped);
            using var run = Earlier(RunFolder("20260102000000"));
            using (run.BeginItem("7", "Task")) { run.MarkChecked(WorkItemReviewCategory.LinkSkipped); run.CompleteCurrentItem(); }
            Assert.IsFalse(File.Exists(run.FilePath), "the broken line may be a problem that was not checked again");
        }

        [TestMethod, TestCategory("L0")]
        public void ReadEntries_OnlyLastLineBroken_IsNotReportedAsSkippedEarlierLine()
        {
            using (var first = Earlier(RunFolder("20260101000000")))
            {
                first.Record(WorkItemReviewCategory.Failed, "boom", "fix", sourceId: "7");
            }
            var file = Path.Combine(RunFolder("20260101000000"), WorkItemReviewLog.FileName);
            File.AppendAllText(file, "{\"sourceId\":\"8\",\"cat");
            Assert.AreEqual(1, WorkItemReviewLog.ReadEntries(file, out var skipped).Count);
            Assert.IsFalse(skipped);
        }

        [TestMethod, TestCategory("L0")]
        public void CompleteCurrentItem_CatchUpFieldMapFailure_ResolvesWhenMapsRunAgain()
        {
            using (var first = Earlier(RunFolder("20260101000000")))
            {
                using (first.BeginItem("7", "Task", 5)) { first.Record(WorkItemReviewCategory.FieldMapFailed, true, "Field map M (a b) failed: boom", "fix"); }
            }
            Assert.IsNull(WorkItemReviewLog.ReadEntries(Path.Combine(RunFolder("20260101000000"), WorkItemReviewLog.FileName)).Single().Revision);
            using var run = Earlier(RunFolder("20260102000000"));
            using (run.BeginItem("7", "Task")) { run.CompleteCurrentItem(); }
            Assert.IsFalse(File.Exists(run.FilePath), "the maps did not run");
            using (run.BeginItem("7", "Task")) { run.MarkFieldMapRan("Field map Other (x y)"); run.CompleteCurrentItem(); }
            Assert.IsFalse(File.Exists(run.FilePath), "another map ran; the failed map may have been removed from the configuration");
            using (run.BeginItem("7", "Task")) { run.MarkFieldMapRan("Field map M (a b)"); run.CompleteCurrentItem(); }
            Assert.AreEqual(WorkItemReviewCategory.Resolved, WorkItemReviewLog.ReadEntries(run.FilePath).Single().Category);
        }
        [TestMethod, TestCategory("L0")]
        public void CompleteCurrentItem_LinkStillHeld_DoesNotResolve()
        {
            using (var first = Earlier(RunFolder("20260101000000")))
            {
                first.Record(WorkItemReviewCategory.LinkSkipped, "link to 2", "migrate 2", sourceId: "1");
            }
            using var run = Earlier(RunFolder("20260102000000"));
            run.SetItemsToMigrate(new[] { "1", "2" });
            using (run.BeginItem("1", "Task"))
            {
                run.MarkChecked(WorkItemReviewCategory.LinkSkipped);
                run.RecordUnlessMigratedLater("2", WorkItemReviewCategory.LinkSkipped, "link to 2.", "migrate 2");
                run.CompleteCurrentItem();
            }
            Assert.IsFalse(File.Exists(run.FilePath), "the link to 2 is still missing until 2 migrates");
        }
        [TestMethod, TestCategory("L0")]
        public void FlushHeldEntries_RelatedItemNeverReached_EntryIsWritten()
        {
            using var log = Earlier(RunFolder("20260101000000"));
            log.SetItemsToMigrate(new[] { "1", "2" });
            using (log.BeginItem("1", "Task")) { log.RecordUnlessMigratedLater("2", WorkItemReviewCategory.LinkSkipped, "link to 2.", "migrate 2"); }
            Assert.IsFalse(File.Exists(log.FilePath));
            log.FlushHeldEntries();
            Assert.AreEqual("1", WorkItemReviewLog.ReadEntries(log.FilePath).Single().SourceId);
        }

        [TestMethod, TestCategory("L0")]
        public void LogSummary_DoesNotThrowWithOrWithoutEntries()
        {
            using var log = Earlier(RunFolder("20260101000000"));
            log.LogSummary(NullLogger.Instance);
            log.Record(WorkItemReviewCategory.LinkSkipped, "x", "y", sourceId: "1");
            log.LogSummary(NullLogger.Instance);
            Assert.AreEqual(1, log.FileRowCounts["follow-up-links.jsonl"]);
        }
    }
}
