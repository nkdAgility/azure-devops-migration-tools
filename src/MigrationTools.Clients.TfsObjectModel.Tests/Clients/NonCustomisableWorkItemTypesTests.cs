using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.Clients;
using MigrationTools.Tools;

namespace MigrationTools.Tests.Clients
{
    [TestClass]
    public class NonCustomisableWorkItemTypesTests
    {
        private const string Configured = "Custom.ReflectedWorkItemId";
        private const string Fallback = NonCustomisableWorkItemTypes.ReflectedWorkItemIdField;

        private static System.Func<string, bool> Has(params string[] fields) => new HashSet<string>(fields).Contains;

        [TestMethod, TestCategory("L0")]
        public void ResolveField_UsesConfiguredFieldWhenTypeHasIt()
        {
            Assert.AreEqual(Configured, NonCustomisableWorkItemTypes.ResolveField("Shared Steps", Configured, Has(Configured, Fallback)));
        }

        [TestMethod, TestCategory("L0")]
        public void ResolveField_ListedTypeWithoutConfiguredField_UsesIntegrationBuild()
        {
            Assert.AreEqual(Fallback, NonCustomisableWorkItemTypes.ResolveField("shared parameter", Configured, Has(Fallback)));
        }

        [TestMethod, TestCategory("L0")]
        public void ResolveField_OtherTypeWithoutConfiguredField_IsNull()
        {
            Assert.IsNull(NonCustomisableWorkItemTypes.ResolveField("Bug", Configured, Has(Fallback)));
        }

        [TestMethod, TestCategory("L0")]
        public void ResolveField_ListedTypeWithNeitherField_IsNull()
        {
            Assert.IsNull(NonCustomisableWorkItemTypes.ResolveField("Shared Steps", Configured, Has()));
        }

        [TestMethod, TestCategory("L0")]
        public void Types_AreTheSixNonCustomisableTypes()
        {
            CollectionAssert.AreEquivalent(new[] { "Code Review Request", "Code Review Response", "Feedback Request", "Feedback Response", "Shared Parameter", "Shared Steps" },
                new List<string>(NonCustomisableWorkItemTypes.Types));
        }

        [TestMethod, TestCategory("L0")]
        public void LookupQuery_MatchesFieldExactlyAndOnlyListedTypes()
        {
            var wiql = NonCustomisableWorkItemTypes.BuildLookupQuery();
            StringAssert.StartsWith(wiql, "[Microsoft.VSTS.Build.IntegrationBuild] = @idToFind AND [System.WorkItemType] IN (");
            StringAssert.Contains(wiql, "'Shared Steps'");
            StringAssert.Contains(wiql, "'Shared Parameter'");
        }

        [TestMethod, TestCategory("L0")]
        public void IsReflectedWorkItemId_AcceptsOnlyReflectedUrls()
        {
            Assert.IsTrue(TfsReflectedWorkItemId.IsReflectedWorkItemId("https://dev.azure.com/org/proj/_workitems/edit/5"));
            Assert.IsFalse(TfsReflectedWorkItemId.IsReflectedWorkItemId("20261112.3"));
            Assert.IsFalse(TfsReflectedWorkItemId.IsReflectedWorkItemId(null));
        }

        [TestMethod, TestCategory("L0")]
        public void DefaultExcludedTypes_AreTheNonCustomisableTypes()
        {
            var options = new TfsWorkItemTypeValidatorToolOptions();
            options.Normalize();
            CollectionAssert.AreEquivalent(new List<string>(NonCustomisableWorkItemTypes.Types), options.ExcludeWorkItemTypes);
        }

        private static string Remap(string json, params (string From, string To)[] pairs)
        {
            var map = new Dictionary<string, string>();
            foreach (var (from, to) in pairs) { map[from] = to; }
            return SharedParameterDataSource.Remap(json, map);
        }

        [TestMethod, TestCategory("L0")]
        public void Remap_AllThreeFormsAndOnlyExactIds()
        {
            var json = "{\"parameterMap\":[{\"paramName\":\"a\",\"dataSetId\":86},{\"paramName\":\"b\",\"dataSetId\": 861}],\"sharedParameterDataSetIds\":[86, 861],\"sharedParameterDataSetId\":86}";
            Assert.AreEqual(
                "{\"parameterMap\":[{\"paramName\":\"a\",\"dataSetId\":99},{\"paramName\":\"b\",\"dataSetId\": 861}],\"sharedParameterDataSetIds\":[99, 861],\"sharedParameterDataSetId\":99}",
                Remap(json, ("86", "99")));
        }

        [TestMethod, TestCategory("L0")]
        public void Remap_ChainedIdsAreTranslatedOnce()
        {
            var json = "{\"parameterMap\":[{\"dataSetId\":86},{\"dataSetId\":87}],\"sharedParameterDataSetIds\":[86,87]}";
            Assert.AreEqual("{\"parameterMap\":[{\"dataSetId\":87},{\"dataSetId\":99}],\"sharedParameterDataSetIds\":[87,99]}",
                Remap(json, ("86", "87"), ("87", "99")));
        }

        [TestMethod, TestCategory("L0")]
        public void Remap_SwappedIds()
        {
            var json = "{\"parameterMap\":[{\"dataSetId\":10},{\"dataSetId\":20}],\"sharedParameterDataSetIds\":[10,20]}";
            Assert.AreEqual("{\"parameterMap\":[{\"dataSetId\":20},{\"dataSetId\":10}],\"sharedParameterDataSetIds\":[20,10]}",
                Remap(json, ("10", "20"), ("20", "10")));
        }
    }
}

