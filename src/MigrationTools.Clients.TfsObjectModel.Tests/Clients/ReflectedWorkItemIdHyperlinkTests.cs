using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.Clients;
using MigrationTools.Tools;

namespace MigrationTools.Tests.Clients
{
    [TestClass]
    public class ReflectedWorkItemIdHyperlinkTests
    {
        private const string Plain = "https://dev.azure.com/org/my project/_workitems/edit/42";

        [TestMethod, TestCategory("L0")]
        public void ToLocation_EscapesSpacesInProjectName()
        {
            Assert.AreEqual("https://dev.azure.com/org/my%20project/_workitems/edit/42", ReflectedWorkItemIdHyperlink.ToLocation(Plain));
        }

        [TestMethod, TestCategory("L0")]
        public void ToLocation_DoesNotDoubleEscape()
        {
            var once = ReflectedWorkItemIdHyperlink.ToLocation(Plain);
            Assert.AreEqual(once, ReflectedWorkItemIdHyperlink.ToLocation(once));
        }

        [TestMethod, TestCategory("L0")]
        public void ToKey_MatchesEscapedLocationAndPlainId()
        {
            Assert.AreEqual(ReflectedWorkItemIdHyperlink.ToKey(Plain),
                ReflectedWorkItemIdHyperlink.ToKey(ReflectedWorkItemIdHyperlink.ToLocation(Plain)));
        }

        [TestMethod, TestCategory("L0")]
        public void ToKey_IgnoresCaseAndTrailingSlash()
        {
            Assert.AreEqual(ReflectedWorkItemIdHyperlink.ToKey(Plain), ReflectedWorkItemIdHyperlink.ToKey(Plain.ToUpperInvariant() + "/"));
        }

        [TestMethod, TestCategory("L0")]
        public void ToKey_DifferentIdsDiffer()
        {
            Assert.AreNotEqual(ReflectedWorkItemIdHyperlink.ToKey(Plain), ReflectedWorkItemIdHyperlink.ToKey(Plain.Replace("/42", "/421")));
        }

        [TestMethod, TestCategory("L0")]
        public void IsMarker_OnlyExactComment()
        {
            Assert.IsTrue(ReflectedWorkItemIdHyperlink.IsMarker(ReflectedWorkItemIdHyperlink.Comment));
            Assert.IsFalse(ReflectedWorkItemIdHyperlink.IsMarker(null));
            Assert.IsFalse(ReflectedWorkItemIdHyperlink.IsMarker("a link copied from the source"));
        }

        [TestMethod, TestCategory("L0")]
        public void HyperlinkQuery_ListsTypesAndEscapesQuotes()
        {
            var wiql = ReflectedWorkItemIdHyperlinkQuery.Build(new[] { "Shared Steps", "O'Brien" });
            StringAssert.Contains(wiql, "[System.HyperLinkCount] > 0");
            StringAssert.Contains(wiql, "[System.WorkItemType] IN ('Shared Steps', 'O''Brien')");
            StringAssert.Contains(wiql, "[System.TeamProject] = @TeamProject");
        }

        [TestMethod, TestCategory("L0")]
        public void RemapDataSetId_ReplacesOnlyExactId()
        {
            var json = "{\"parameterMap\":[{\"paramName\":\"a\",\"dataSetId\":86},{\"paramName\":\"b\",\"dataSetId\": 861}],\"sharedParameterDataSetIds\":[86]}";
            var result = SharedParameterDataSource.RemapDataSetId(json, "86", "99");
            StringAssert.Contains(result, "\"dataSetId\":99}");
            StringAssert.Contains(result, "\"dataSetId\": 861");
            StringAssert.Contains(result, "[86]");
        }

        [TestMethod, TestCategory("L0")]
        public void Validator_ReportsExcludedTypesWithoutField()
        {
            var types = TfsWorkItemTypeValidatorTool.SelectTypesUsingReflectedHyperlink(
                new[] { ("Shared Steps", false), ("Shared Parameter", true), ("Bug", false), ("Feedback Request", false) },
                new[] { "shared steps", "Shared Parameter", "Feedback Request" });
            CollectionAssert.AreEqual(new[] { "Shared Steps", "Feedback Request" }, types);
        }
    }
}
