using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.Processors;

namespace MigrationTools.Tests.Processors
{
    [TestClass]
    public class MigrationHyperlinkHelperTests
    {
        private const string Url = "https://dev.azure.com/org/proj/_workitems/edit/1";
        private const string OldUrl = "https://tfs.old/DefaultCollection/proj/_workitems/edit/1";
        private static readonly string[] NoSourceLinks = new string[0];

        [TestMethod, TestCategory("L0")]
        public void Plan_NoHyperlinks_Adds()
        {
            var plan = MigrationHyperlinkHelper.Plan(new List<(string, string)>(), NoSourceLinks, Url);
            Assert.IsTrue(plan.Add);
            Assert.AreEqual(0, plan.Remove.Count);
        }

        [TestMethod, TestCategory("L0")]
        public void Plan_AlreadyPresent_DoesNothing()
        {
            var links = new List<(string, string)> { (Url.ToUpperInvariant(), MigrationHyperlinkHelper.Comment) };
            var plan = MigrationHyperlinkHelper.Plan(links, NoSourceLinks, Url);
            Assert.IsFalse(plan.Add);
            Assert.AreEqual(0, plan.Remove.Count);
        }

        [TestMethod, TestCategory("L0")]
        public void Plan_PresentWithOtherComment_NotAddedAgain()
        {
            var links = new List<(string, string)> { (Url, "copied") };
            Assert.IsFalse(MigrationHyperlinkHelper.Plan(links, NoSourceLinks, Url).Add);
        }

        [TestMethod, TestCategory("L0")]
        public void Plan_SourceMoved_ReplacesOldMigrationLink()
        {
            var links = new List<(string, string)> { ("https://example.com/doc", "spec"), (OldUrl, MigrationHyperlinkHelper.Comment) };
            var plan = MigrationHyperlinkHelper.Plan(links, NoSourceLinks, Url);
            Assert.IsTrue(plan.Add);
            CollectionAssert.AreEqual(new[] { 1 }, new List<int>(plan.Remove));
        }

        [TestMethod, TestCategory("L0")]
        public void Plan_MigrationLinkCopiedFromSource_Kept()
        {
            var links = new List<(string, string)> { (OldUrl, MigrationHyperlinkHelper.Comment) };
            var plan = MigrationHyperlinkHelper.Plan(links, new[] { OldUrl }, Url);
            Assert.IsTrue(plan.Add);
            Assert.AreEqual(0, plan.Remove.Count);
        }

        [TestMethod, TestCategory("L0")]
        public void IsMigrationHyperlink_MatchesOnlyTheComment()
        {
            Assert.IsTrue(MigrationHyperlinkHelper.IsMigrationHyperlink("Migrated from source"));
            Assert.IsFalse(MigrationHyperlinkHelper.IsMigrationHyperlink("something else"));
            Assert.IsFalse(MigrationHyperlinkHelper.IsMigrationHyperlink(null));
        }
    }
}
