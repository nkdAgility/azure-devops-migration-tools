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
    }
}
