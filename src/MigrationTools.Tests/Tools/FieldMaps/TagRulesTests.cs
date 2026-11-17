using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.Tools;

namespace MigrationTools.Tests.Tools.FieldMaps
{
    [TestClass]
    public class TagRulesTests
    {
        private static List<string> L(params string[] items) => new List<string>(items);

        [TestMethod, TestCategory("L0")]
        public void Split_TrimsRemovesEmptyAndDuplicates()
        {
            CollectionAssert.AreEqual(L("A", "B", "b"), TagRules.Split(" A ;B;;A; b "));
            Assert.AreEqual(0, TagRules.Split(null).Count);
            Assert.AreEqual(0, TagRules.Split("  ").Count);
        }

        [TestMethod, TestCategory("L0")]
        public void Filter_IncludeOnly_KeepsOnlyApprovedTags()
        {
            var result = TagRules.Filter(L("Approved", "Random", "Release"), L("Approved", "Release"), L());
            CollectionAssert.AreEqual(L("Approved", "Release"), result.Kept);
            CollectionAssert.AreEqual(L("Random"), result.Dropped);
        }

        [TestMethod, TestCategory("L0")]
        public void Filter_IsCaseSensitive()
        {
            var result = TagRules.Filter(L("approved", "Approved"), L("Approved"), L());
            CollectionAssert.AreEqual(L("Approved"), result.Kept);
            CollectionAssert.AreEqual(L("approved"), result.Dropped);
        }

        [TestMethod, TestCategory("L0")]
        public void Filter_ExcludeOnly_KeepsEverythingElse()
        {
            var result = TagRules.Filter(L("A", "Secret", "B"), L(), L("Secret"));
            CollectionAssert.AreEqual(L("A", "B"), result.Kept);
            CollectionAssert.AreEqual(L("Secret"), result.Dropped);
        }

        [TestMethod, TestCategory("L0")]
        public void Filter_ExcludeWinsOverInclude()
        {
            var result = TagRules.Filter(L("Team-A", "Team-Legacy", "Other"), L("regex:^Team-"), L("Team-Legacy"));
            CollectionAssert.AreEqual(L("Team-A"), result.Kept);
            CollectionAssert.AreEqual(L("Team-Legacy", "Other"), result.Dropped);
        }

        [TestMethod, TestCategory("L0")]
        public void Filter_NullLists_KeepsEverything()
        {
            var result = TagRules.Filter(L("A"), null, null);
            CollectionAssert.AreEqual(L("A"), result.Kept);
        }

        [TestMethod, TestCategory("L0")]
        public void Map_RenamesExactAndKeepsUnmatched()
        {
            var result = TagRules.Map(L("Bug-Bash", "bug-bash", "Other"), new List<TagMapping> { new TagMapping { source = "Bug-Bash", target = "BugBash" } });
            CollectionAssert.AreEqual(L("BugBash", "bug-bash", "Other"), result.Tags);
            Assert.AreEqual(1, result.Renamed.Count);
        }

        [TestMethod, TestCategory("L0")]
        public void Map_RegexWithGroups_AndRemovesDuplicates()
        {
            var mappings = new List<TagMapping>
            {
                new TagMapping { source = "regex:^Sprint (\\d+)$", target = "S$1" },
                new TagMapping { source = "regex:^v1\\..*$", target = "Legacy" }
            };
            var result = TagRules.Map(L("Sprint 12", "v1.2", "v1.3", "Legacy"), mappings);
            CollectionAssert.AreEqual(L("S12", "Legacy"), result.Tags);
        }

        [TestMethod, TestCategory("L0")]
        public void Map_EmptyReplacement_KeepsOriginalTag()
        {
            var result = TagRules.Map(L("Old"), new List<TagMapping> { new TagMapping { source = "regex:^Old(.*)$", target = "$1" } });
            CollectionAssert.AreEqual(L("Old"), result.Tags);
            Assert.AreEqual(0, result.Renamed.Count);
        }

        [TestMethod, TestCategory("L0")]
        public void Map_FirstMatchingMappingWins()
        {
            var mappings = new List<TagMapping>
            {
                new TagMapping { source = "A", target = "First" },
                new TagMapping { source = "regex:.*", target = "Second" }
            };
            CollectionAssert.AreEqual(L("First"), TagRules.Map(L("A"), mappings).Tags);
        }

        [TestMethod, TestCategory("L0")]
        public void Validate_RejectsBadPatternAndEmptyEntries()
        {
            Assert.ThrowsExactly<ArgumentException>(() => TagRules.Validate(L("regex:(unclosed"), "includeTags"));
            Assert.ThrowsExactly<ArgumentException>(() => TagRules.Validate(L(" "), "includeTags"));
            Assert.ThrowsExactly<ArgumentException>(() => TagRules.Validate(L("regex:"), "includeTags"));
            TagRules.Validate(L("Ok", "regex:^ok$"), "includeTags");
            TagRules.Validate(null, "includeTags");
        }

        [TestMethod, TestCategory("L0")]
        public void Options_SetExampleConfigDefaults()
        {
            var filter = new TagFilterMapOptions();
            filter.SetExampleConfigDefaults();
            CollectionAssert.Contains(filter.includeTags, "Approved");
            CollectionAssert.Contains(filter.ApplyTo, "*");

            var mapping = new TagMappingMapOptions();
            mapping.SetExampleConfigDefaults();
            Assert.AreEqual(2, mapping.mappings.Count);
        }
    }
}
