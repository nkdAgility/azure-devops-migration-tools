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
    
        [TestMethod, TestCategory("L0")]
        public void IsGeneratedBacklink_OnlyMatchesCommentAndSourceUrl()
        {
            Assert.IsTrue(MigrationHyperlinkHelper.IsGeneratedBacklink(Url.ToUpperInvariant(), MigrationHyperlinkHelper.Comment, Url));
            Assert.IsFalse(MigrationHyperlinkHelper.IsGeneratedBacklink(OldUrl, MigrationHyperlinkHelper.Comment, Url), "a copied marked link to another URL is an ordinary link");
            Assert.IsFalse(MigrationHyperlinkHelper.IsGeneratedBacklink(Url, "something else", Url));
            Assert.IsFalse(MigrationHyperlinkHelper.IsGeneratedBacklink(Url, MigrationHyperlinkHelper.Comment, null));
        }

        [TestMethod, TestCategory("L0")]
        public void GenerateMigrationHyperlink_FalseSurvivesSerialisation()
        {
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(new TfsWorkItemMigrationProcessorOptions { GenerateMigrationHyperlink = false });
            StringAssert.Contains(json, "\"GenerateMigrationHyperlink\":false");
            var back = Newtonsoft.Json.JsonConvert.DeserializeObject<TfsWorkItemMigrationProcessorOptions>(json);
            Assert.IsFalse(back.GenerateMigrationHyperlink);
            Assert.IsFalse(Newtonsoft.Json.JsonConvert.SerializeObject(new TfsWorkItemMigrationProcessorOptions()).Contains("GenerateMigrationHyperlink"), "the default true is omitted");
        }
    }
}