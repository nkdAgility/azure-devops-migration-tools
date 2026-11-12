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
        public void DefaultExcludedTypes_AreTheNonCustomisableTypes()
        {
            var options = new TfsWorkItemTypeValidatorToolOptions();
            options.Normalize();
            CollectionAssert.AreEquivalent(new List<string>(NonCustomisableWorkItemTypes.Types), options.ExcludeWorkItemTypes);
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
    }
}
