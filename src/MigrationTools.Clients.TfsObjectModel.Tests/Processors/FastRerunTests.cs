using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.Clients;
using MigrationTools.Processors;

namespace MigrationTools.Tests.Processors
{
    [TestClass]
    public class ReflectedWorkItemIdMapTests
    {
        private static readonly Uri Source = new Uri("https://dev.azure.com/source-org/");

        [TestMethod, TestCategory("L0")]
        public void TryGetTargetId_ByReflectedId_FindsTarget()
        {
            var map = new ReflectedWorkItemIdMap();
            map.Add("https://dev.azure.com/source-org/Project A/_workitems/edit/42", 1042);

            Assert.IsTrue(map.TryGetTargetId("https://dev.azure.com/source-org/Project A/_workitems/edit/42", out var id));
            Assert.AreEqual(1042, id);
        }

        [TestMethod, TestCategory("L0")]
        public void TryGetTargetId_BySourceCollectionAndId_IgnoresProject()
        {
            var map = new ReflectedWorkItemIdMap();
            map.Add("https://dev.azure.com/source-org/Old Project/_workitems/edit/7", 107);

            Assert.IsTrue(map.TryGetTargetId(Source, 7, out var id));
            Assert.AreEqual(107, id);
        }

        [TestMethod, TestCategory("L0")]
        public void TryGetTargetId_OtherCollection_NotFound()
        {
            var map = new ReflectedWorkItemIdMap();
            map.Add("https://dev.azure.com/other-org/P/_workitems/edit/7", 107);

            Assert.IsFalse(map.TryGetTargetId(Source, 7, out _));
            Assert.IsFalse(map.IsAmbiguous("https://dev.azure.com/source-org/P/_workitems/edit/7"));
        }

        [TestMethod, TestCategory("L0")]
        public void Add_TwoTargetsForOneSource_IsAmbiguousAndNotAnswered()
        {
            var map = new ReflectedWorkItemIdMap();
            map.Add("https://dev.azure.com/source-org/P/_workitems/edit/9", 109);
            map.Add("https://dev.azure.com/source-org/P/_workitems/edit/9", 209);
            map.Add("https://dev.azure.com/source-org/P/_workitems/edit/9", 109);

            Assert.IsFalse(map.TryGetTargetId(Source, 9, out _));
            Assert.IsTrue(map.IsAmbiguous("https://dev.azure.com/source-org/P/_workitems/edit/9"));
            Assert.AreEqual(1, map.AmbiguousCount);
            Assert.AreEqual(0, map.Count);
        }

        [TestMethod, TestCategory("L0")]
        public void Add_SameTargetTwice_IsNotAmbiguous()
        {
            var map = new ReflectedWorkItemIdMap();
            map.Add("https://dev.azure.com/source-org/P/_workitems/edit/9", 109);
            map.Add("https://dev.azure.com/source-org/P/_workitems/edit/9", 109);

            Assert.IsTrue(map.TryGetTargetId(Source, 9, out var id));
            Assert.AreEqual(109, id);
        }

        [TestMethod, TestCategory("L0")]
        public void Add_ValueThatIsNotAReflectedId_IsIgnored()
        {
            var map = new ReflectedWorkItemIdMap();
            map.Add("Build 20261119.1", 5);
            map.Add(null, 6);

            Assert.AreEqual(0, map.Count);
            Assert.IsTrue(map.IsAmbiguous("Build 20261119.1"));
        }

        [TestMethod, TestCategory("L0")]
        public void TryGetTargetId_IgnoresCase()
        {
            var map = new ReflectedWorkItemIdMap();
            map.Add("https://dev.azure.com/Source-Org/P/_workitems/edit/3", 103);

            Assert.IsTrue(map.TryGetTargetId(Source, 3, out var id));
            Assert.AreEqual(103, id);
        }
    }

    [TestClass]
    public class AttachmentDiffTests
    {
        [TestMethod, TestCategory("L0")]
        public void FirstMissing_SameAttachments_Null()
        {
            Assert.IsNull(AttachmentDiff.FirstMissing(new[] { ("a.png", 10L), ("b.txt", 5L) }, new[] { ("B.TXT", 5L), ("a.png", 10L) }));
        }

        [TestMethod, TestCategory("L0")]
        public void FirstMissing_DifferentSize_ReportsIt()
        {
            Assert.AreEqual("a.png", AttachmentDiff.FirstMissing(new[] { ("a.png", 10L) }, new[] { ("a.png", 11L) }));
        }

        [TestMethod, TestCategory("L0")]
        public void FirstMissing_DuplicateInSourceOnlyOnceInTarget_ReportsIt()
        {
            Assert.AreEqual("a.png", AttachmentDiff.FirstMissing(new[] { ("a.png", 10L), ("a.png", 10L) }, new[] { ("a.png", 10L) }));
        }

        [TestMethod, TestCategory("L0")]
        public void FirstMissing_ExtraInTarget_Null()
        {
            Assert.IsNull(AttachmentDiff.FirstMissing(new[] { ("a.png", 10L) }, new[] { ("a.png", 10L), ("c.png", 1L) }));
        }
    }

    [TestClass]
    public class RerunCountsTests
    {
        [TestMethod, TestCategory("L0")]
        public void Counts_OnlyFinishedItems()
        {
            var counts = new RerunCounts();
            counts.Begin(); counts.Set(RerunCounts.Outcome.InSync); counts.Succeeded();
            counts.Begin(); counts.Set(RerunCounts.Outcome.InSync); counts.Succeeded();
            counts.Begin(); counts.Set(RerunCounts.Outcome.Updated); counts.Succeeded();
            counts.Begin(); counts.Set(RerunCounts.Outcome.Created); counts.Succeeded();
            counts.Begin(); counts.Set(RerunCounts.Outcome.Updated); counts.Fail();
            counts.Begin(); counts.Set(RerunCounts.Outcome.Updated); counts.Set(RerunCounts.Outcome.Checked); counts.Succeeded();
            counts.Begin(); counts.Succeeded();

            Assert.AreEqual(2, counts.InSync);
            Assert.AreEqual(1, counts.Checked);
            Assert.AreEqual(1, counts.Updated);
            Assert.AreEqual(1, counts.Created);
            Assert.AreEqual(1, counts.Failed);
            Assert.AreEqual("2 in sync and untouched, 1 checked with nothing to change, 1 updated, 1 created, 1 failed", counts.ToString());
        }
    }
}
