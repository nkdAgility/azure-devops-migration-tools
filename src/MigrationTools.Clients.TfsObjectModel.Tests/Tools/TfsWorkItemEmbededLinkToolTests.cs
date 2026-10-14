using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.DataContracts;
using MigrationTools.Tools;

namespace MigrationTools.Tests.Tools
{
    [TestClass]
    public class TfsWorkItemEmbededLinkToolTests
    {
        private class TargetIdentity
        {
            public string DisplayName { get; set; }
            public string UniqueName { get; set; }
        }

        private static readonly TargetIdentity Test = new() { DisplayName = "Test", UniqueName = "test@target.com" };
        private static readonly TargetIdentity Tester = new() { DisplayName = "Tester", UniqueName = "tester@target.com" };
        private static readonly TargetIdentity SharedA = new() { DisplayName = "Shared Name", UniqueName = "a@target.com" };
        private static readonly TargetIdentity SharedB = new() { DisplayName = "Shared Name", UniqueName = "b@target.com" };
        private static readonly List<TargetIdentity> Targets = [Test, Tester, SharedA, SharedB];

        private static TargetIdentity Find(string mention, IDictionary<string, string> map, out string mappedName) =>
            TfsWorkItemEmbededLinkTool.FindMentionIdentity(mention, map, Targets, i => i.DisplayName, i => i.UniqueName, out mappedName);

        private static Dictionary<string, string> Map(params (string Key, string Value)[] entries)
        {
            var map = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
            foreach (var (key, value) in entries)
            {
                map[key] = value;
            }
            return map;
        }

        private const string SourceOrg = "https://dev.azure.com/source/";
        private const string TargetOrg = "https://dev.azure.com/target/";

        [TestMethod, TestCategory("L0")]
        public void IsTargetWorkItemUrl_DifferentCollections_RecognisesTargetUrl()
        {
            // Issue #3212: a re-sync must not look up an already rewritten target id on the source.
            Assert.IsTrue(TfsWorkItemEmbededLinkTool.IsTargetWorkItemUrl("https://dev.azure.com/target/Tgt/_workitems/edit/105651", SourceOrg, "Src", TargetOrg, "Tgt"));
            Assert.IsFalse(TfsWorkItemEmbededLinkTool.IsTargetWorkItemUrl("https://dev.azure.com/source/Src/_workitems/edit/42", SourceOrg, "Src", TargetOrg, "Tgt"));
            Assert.IsFalse(TfsWorkItemEmbededLinkTool.IsTargetWorkItemUrl("https://dev.azure.com/targetother/Tgt/_workitems/edit/42", SourceOrg, "Src", TargetOrg, "Tgt"));
        }

        [TestMethod, TestCategory("L0")]
        public void IsTargetWorkItemUrl_SameCollection_UsesProject()
        {
            Assert.IsTrue(TfsWorkItemEmbededLinkTool.IsTargetWorkItemUrl("https://dev.azure.com/org/Target%20Project/_workitems/edit/7", "https://dev.azure.com/org", "Src", "https://dev.azure.com/org", "Target Project"));
            Assert.IsFalse(TfsWorkItemEmbededLinkTool.IsTargetWorkItemUrl("https://dev.azure.com/org/Src/_workitems/edit/7", "https://dev.azure.com/org", "Src", "https://dev.azure.com/org", "Target Project"));
            Assert.IsFalse(TfsWorkItemEmbededLinkTool.IsTargetWorkItemUrl("https://dev.azure.com/org/Same/_workitems/edit/7", "https://dev.azure.com/org", "Same", "https://dev.azure.com/org", "Same"));
            Assert.IsFalse(TfsWorkItemEmbededLinkTool.IsTargetWorkItemUrl("https://dev.azure.com/other/Target%20Project/_workitems/edit/7", "https://dev.azure.com/org", "Src", "https://dev.azure.com/org", "Target Project"));
        }

        [TestMethod, TestCategory("L0")]
        public void IsTargetWorkItemUrl_LegacyHostForm_IsSameOrganisation()
        {
            Assert.IsTrue(TfsWorkItemEmbededLinkTool.IsTargetWorkItemUrl("https://target.visualstudio.com/Tgt/_workitems/edit/7", SourceOrg, "Src", TargetOrg, "Tgt"));
            Assert.IsTrue(TfsWorkItemEmbededLinkTool.IsTargetWorkItemUrl("https://dev.azure.com/target/Tgt/_workitems/edit/7", "https://source.visualstudio.com/", "Src", "https://target.visualstudio.com/", "Tgt"));
            Assert.IsFalse(TfsWorkItemEmbededLinkTool.IsTargetWorkItemUrl("https://source.visualstudio.com/Src/_workitems/edit/7", SourceOrg, "Src", TargetOrg, "Tgt"));
        }

        [TestMethod, TestCategory("L0")]
        public void ReplaceProject_EncodedProjectInHref_IsRewrittenSoResyncRecognisesIt()
        {
            var rewritten = TfsWorkItemEmbededLinkTool.ReplaceProject("<a href=\"https://dev.azure.com/org/Source%20Project/_workitems/edit/9/\" data-vss-mention=\"version:1.0\">#9</a>", "Source Project", "Target Project");
            StringAssert.Contains(rewritten, "/Target%20Project/_workitems/edit/9/");
            Assert.IsTrue(TfsWorkItemEmbededLinkTool.IsTargetWorkItemUrl("https://dev.azure.com/org/Target%20Project/_workitems/edit/9/", "https://dev.azure.com/org/", "Source Project", "https://dev.azure.com/org/", "Target Project"));
        }

        [TestMethod, TestCategory("L0")]
        public void FindMentionIdentity_MappedToDisplayName_ReturnsMappedTarget()
        {
            // Issue #2866: "Tester" is mapped to "Test", so the mention must not resolve to the target user named "Tester".
            Assert.AreSame(Test, Find("Tester", Map(("Tester", "Test")), out var mappedName));
            Assert.AreEqual("Test", mappedName);
        }

        [TestMethod, TestCategory("L0")]
        public void FindMentionIdentity_MappedToUniqueName_ReturnsTarget()
        {
            Assert.AreSame(Test, Find("Old Name", Map(("Old Name", "test@target.com")), out _));
        }

        [TestMethod, TestCategory("L0")]
        public void FindMentionIdentity_MappedToQualifiedName_ResolvesByUniqueNameOnly()
        {
            Assert.AreSame(SharedB, Find("Old Name", Map(("Old Name", "Shared Name <b@target.com>")), out _));
            Assert.IsNull(Find("Old Name", Map(("Old Name", "Shared Name <missing@target.com>")), out _));
        }

        [TestMethod, TestCategory("L0")]
        public void FindMentionIdentity_QualifiedSourceKey_MapsPlainMention()
        {
            Assert.AreSame(Test, Find("Old Name", Map(("Old Name <old@source.com>", "Test")), out _));
        }

        [TestMethod, TestCategory("L0")]
        public void FindMentionIdentity_MappingIgnoresCase()
        {
            Assert.AreSame(Test, Find("tester", Map(("Tester", "Test")), out _));
        }

        [TestMethod, TestCategory("L0")]
        public void FindMentionIdentity_Unmapped_UsesExactDisplayName()
        {
            Assert.AreSame(Tester, Find("Tester", Map(("Someone", "Test")), out var mappedName));
            Assert.IsNull(mappedName);
            // Unmapped mentions keep the previous exact, case-sensitive display name match.
            Assert.IsNull(Find("tester", Map(), out _));
            Assert.IsNull(Find("test@target.com", Map(), out _));
        }

        [TestMethod, TestCategory("L0")]
        public void FindMentionIdentity_MappingDisabled_UsesExactDisplayName()
        {
            Assert.AreSame(Tester, Find("Tester", null, out var mappedName));
            Assert.IsNull(mappedName);
        }

        [TestMethod, TestCategory("L0")]
        public void FindMentionIdentity_IdentityMapDataFormat_MapsDisplayNameAndAccountName()
        {
            var map = TfsUserMappingTool.ToUserMapDictionary(new List<IdentityMapData>
            {
                new IdentityMapData
                {
                    Source = new IdentityItemData { DisplayName = "Old Name", AccountName = "DOMAIN\\old" },
                    Target = new IdentityItemData { DisplayName = "Test" }
                }
            });
            Assert.AreSame(Test, Find("Old Name", map, out _));
            Assert.AreSame(Test, Find("DOMAIN\\old", map, out _));
        }
    }
}
