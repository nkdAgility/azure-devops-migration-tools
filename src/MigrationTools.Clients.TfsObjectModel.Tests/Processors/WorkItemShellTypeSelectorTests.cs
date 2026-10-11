using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MigrationTools.Processors.Tests
{
    [TestClass]
    public class WorkItemShellTypeSelectorTests
    {
        private static readonly Dictionary<string, string> NoMappings = new Dictionary<string, string>();

        [TestMethod, TestCategory("L0")]
        public void Select_FirstTypeUsable_UsesFirstType()
        {
            var s = WorkItemShellTypeSelector.Select("Bug", "User Story", NoMappings, t => true);
            Assert.AreEqual("Bug", s.ShellType);
            Assert.IsFalse(s.FellBackToFinalType);
        }

        [TestMethod, TestCategory("L0")]
        public void Select_FirstTypeDisabled_UsesFinalType()
        {
            var s = WorkItemShellTypeSelector.Select("Bug", "User Story", NoMappings, t => t != "Bug");
            Assert.AreEqual("User Story", s.ShellType);
            Assert.IsTrue(s.FellBackToFinalType);
        }

        [TestMethod, TestCategory("L0")]
        public void Select_NoTypeChange_NeverFallsBack()
        {
            var s = WorkItemShellTypeSelector.Select("Bug", "Bug", NoMappings, t => false);
            Assert.AreEqual("Bug", s.ShellType);
            Assert.IsFalse(s.FellBackToFinalType);
        }

        [TestMethod, TestCategory("L0")]
        public void Select_BothUnusable_KeepsFirstType()
        {
            var s = WorkItemShellTypeSelector.Select("Bug", "User Story", NoMappings, t => false);
            Assert.AreEqual("Bug", s.ShellType);
            Assert.IsFalse(s.FellBackToFinalType);
        }

        [TestMethod, TestCategory("L0")]
        public void Select_AppliesMappingsToBothTypes()
        {
            var mappings = new Dictionary<string, string> { { "Bug", "Defect" }, { "User Story", "Product Backlog Item" } };
            var usable = WorkItemShellTypeSelector.Select("Bug", "User Story", mappings, t => true);
            Assert.AreEqual("Defect", usable.ShellType);

            var fallback = WorkItemShellTypeSelector.Select("Bug", "User Story", mappings, t => t == "Product Backlog Item");
            Assert.AreEqual("Product Backlog Item", fallback.ShellType);
            Assert.AreEqual("Defect", fallback.FirstType);
            Assert.IsTrue(fallback.FellBackToFinalType);
        }

        [TestMethod, TestCategory("L0")]
        public void Select_MappedToSameType_NoFallback()
        {
            var mappings = new Dictionary<string, string> { { "Bug", "User Story" } };
            var s = WorkItemShellTypeSelector.Select("Bug", "User Story", mappings, t => false);
            Assert.AreEqual("User Story", s.ShellType);
            Assert.IsFalse(s.FellBackToFinalType);
        }

        [TestMethod, TestCategory("L0")]
        public void SelectRevisionType_UsableType_UsesRevisionType()
        {
            Assert.AreEqual("User Story", WorkItemShellTypeSelector.SelectRevisionType("User Story", "Bug", NoMappings, t => true));
        }

        [TestMethod, TestCategory("L0")]
        public void SelectRevisionType_UnusableType_KeepsCurrentType()
        {
            Assert.AreEqual("User Story", WorkItemShellTypeSelector.SelectRevisionType("Bug", "User Story", NoMappings, t => t != "Bug"));
        }

        [TestMethod, TestCategory("L0")]
        public void SelectRevisionType_SameAsCurrent_DoesNotCheckUsability()
        {
            var checkedTypes = new List<string>();
            var result = WorkItemShellTypeSelector.SelectRevisionType("Bug", "Bug", NoMappings, t => { checkedTypes.Add(t); return false; });
            Assert.AreEqual("Bug", result);
            Assert.AreEqual(0, checkedTypes.Count);
        }

        [TestMethod, TestCategory("L0")]
        public void SelectRevisionType_AppliesMapping()
        {
            var mappings = new Dictionary<string, string> { { "Bug", "Defect" } };
            Assert.AreEqual("Defect", WorkItemShellTypeSelector.SelectRevisionType("Bug", "User Story", mappings, t => true));
        }

        // Simulates the shell selection plus revision replay and returns the target type after each revision.
        private static List<string> Replay(string[] revisionTypes, System.Func<string, bool> isUsable)
        {
            var current = WorkItemShellTypeSelector.Select(revisionTypes[0], revisionTypes[revisionTypes.Length - 1], NoMappings, isUsable).ShellType;
            var result = new List<string>();
            foreach (var type in revisionTypes)
            {
                current = WorkItemShellTypeSelector.SelectRevisionType(type, current, NoMappings, isUsable);
                result.Add(current);
            }
            return result;
        }

        [TestMethod, TestCategory("L0")]
        public void Replay_DisabledFirstTypeOverSeveralRevisions_NeverReturnsToDisabledType()
        {
            var types = Replay(new[] { "Bug", "Bug", "Bug", "User Story" }, t => t != "Bug");
            CollectionAssert.AreEqual(new[] { "User Story", "User Story", "User Story", "User Story" }, types);
        }

        [TestMethod, TestCategory("L0")]
        public void Replay_DisabledIntermediateType_IsSkipped()
        {
            var types = Replay(new[] { "Bug", "Issue", "User Story" }, t => t != "Issue");
            CollectionAssert.AreEqual(new[] { "Bug", "Bug", "User Story" }, types);
        }

        [TestMethod, TestCategory("L0")]
        public void Replay_AllTypesUsable_FollowsSourceHistory()
        {
            var types = Replay(new[] { "Bug", "Issue", "User Story" }, t => true);
            CollectionAssert.AreEqual(new[] { "Bug", "Issue", "User Story" }, types);
        }
    }
}
