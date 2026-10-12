using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.Processors;

namespace MigrationTools.Tests.Processors
{
    [TestClass]
    public class RevisionArtifactDiffTests
    {
        private static (List<string> Added, List<string> Removed) Diff(string[] previous, string[] current)
            => RevisionArtifactDiff.Compare(previous, current, x => x);

        [TestMethod, TestCategory("L0")]
        public void Compare_FirstRevision_AllAddedNothingRemoved()
        {
            var (added, removed) = Diff(null, new[] { "a", "b" });
            CollectionAssert.AreEqual(new[] { "a", "b" }, added);
            Assert.AreEqual(0, removed.Count);
        }

        [TestMethod, TestCategory("L0")]
        public void Compare_Unchanged_NothingAddedOrRemoved()
        {
            var (added, removed) = Diff(new[] { "a", "b" }, new[] { "b", "a" });
            Assert.AreEqual(0, added.Count);
            Assert.AreEqual(0, removed.Count);
        }

        [TestMethod, TestCategory("L0")]
        public void Compare_AddAndRemoveInSameRevision()
        {
            var (added, removed) = Diff(new[] { "a", "b" }, new[] { "b", "c" });
            CollectionAssert.AreEqual(new[] { "c" }, added);
            CollectionAssert.AreEqual(new[] { "a" }, removed);
        }

        [TestMethod, TestCategory("L0")]
        public void Compare_RemovedToEmpty()
        {
            var (added, removed) = Diff(new[] { "a" }, new string[0]);
            Assert.AreEqual(0, added.Count);
            CollectionAssert.AreEqual(new[] { "a" }, removed);
        }

        [TestMethod, TestCategory("L0")]
        public void Compare_NullCurrent_RemovesAll()
        {
            var (added, removed) = Diff(new[] { "a" }, null);
            Assert.AreEqual(0, added.Count);
            CollectionAssert.AreEqual(new[] { "a" }, removed);
        }

        [TestMethod, TestCategory("L0")]
        public void Compare_DuplicateKeys_TreatedAsOne()
        {
            var (added, removed) = Diff(null, new[] { "a", "a" });
            CollectionAssert.AreEqual(new[] { "a" }, added);
            Assert.AreEqual(0, removed.Count);
        }

        [TestMethod, TestCategory("L0")]
        public void Compare_ReplayedAcrossRevisions_AddedAndRemovedAtTheRightRevision()
        {
            // Source history: r1 none, r2 +link +att, r3 no change, r4 -link, r5 +link again
            var revisions = new[]
            {
                new string[0],
                new[] { RevisionArtifactDiff.HyperlinkKey("https://x/1"), RevisionArtifactDiff.AttachmentKey(7) },
                new[] { RevisionArtifactDiff.HyperlinkKey("https://x/1"), RevisionArtifactDiff.AttachmentKey(7) },
                new[] { RevisionArtifactDiff.AttachmentKey(7) },
                new[] { RevisionArtifactDiff.AttachmentKey(7), RevisionArtifactDiff.HyperlinkKey("HTTPS://X/1") },
            };
            string[] previous = null;
            var log = new List<string>();
            for (int i = 0; i < revisions.Length; i++)
            {
                var (added, removed) = Diff(previous, revisions[i]);
                log.AddRange(added.Select(a => $"r{i + 1}+{a}"));
                log.AddRange(removed.Select(r => $"r{i + 1}-{r}"));
                previous = revisions[i];
            }
            CollectionAssert.AreEqual(new[]
            {
                "r2+H|https://x/1", "r2+A|7",
                "r4-H|https://x/1",
                "r5+H|https://x/1",
            }, log);
        }

        [TestMethod, TestCategory("L0")]
        public void ResumeFromMiddle_UsesPreviousSourceRevisionAsBaseline()
        {
            // Target already has r1..r2; replay starts at r3 where the link was removed.
            var r2 = new[] { RevisionArtifactDiff.HyperlinkKey("https://x/1") };
            var r3 = new string[0];
            var (added, removed) = Diff(r2, r3);
            Assert.AreEqual(0, added.Count);
            CollectionAssert.AreEqual(new[] { "H|https://x/1" }, removed);
        }

        [TestMethod, TestCategory("L0")]
        public void HyperlinkKey_IgnoresCaseAndQuotes()
        {
            Assert.AreEqual(RevisionArtifactDiff.HyperlinkKey("https://Example.com/A"), RevisionArtifactDiff.HyperlinkKey("\"https://example.com/a\""));
        }

        [TestMethod, TestCategory("L0")]
        public void RelatedLinkKey_DistinguishesLinkTypeAndTarget()
        {
            Assert.AreNotEqual(
                RevisionArtifactDiff.RelatedLinkKey("System.LinkTypes.Hierarchy-Reverse", 5),
                RevisionArtifactDiff.RelatedLinkKey("System.LinkTypes.Related-Forward", 5));
            Assert.AreNotEqual(
                RevisionArtifactDiff.RelatedLinkKey("System.LinkTypes.Related-Forward", 5),
                RevisionArtifactDiff.RelatedLinkKey("System.LinkTypes.Related-Forward", 6));
        }

        [TestMethod, TestCategory("L0")]
        public void ExternalLinkKey_IgnoresCase()
        {
            Assert.AreEqual(RevisionArtifactDiff.ExternalLinkKey("vstfs:///Git/Commit/A"), RevisionArtifactDiff.ExternalLinkKey("vstfs:///git/commit/a"));
        }

        [TestMethod, TestCategory("L0")]
        public void AttachmentOriginalIdMarker_MatchesExistingCommentFormat()
        {
            Assert.AreEqual("[originalId:42]", RevisionArtifactDiff.AttachmentOriginalIdMarker(42));
        }
    }
}
