using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.FieldMaps.AzureDevops.ObjectModel;
using MigrationTools.Tools;

namespace MigrationTools.Tests.Tools
{
    [TestClass]
    public class FieldMapBaseReviewKeyTests
    {
        [TestMethod, TestCategory("L0")]
        public void ComputeConfigHash_TwoInstancesOfOneMapType_Differ()
        {
            var first = new TreeToTagFieldMapOptions { toSkip = 1, ApplyTo = new() { "Task" } };
            var second = new TreeToTagFieldMapOptions { toSkip = 2, ApplyTo = new() { "Task" } };
            Assert.AreNotEqual(FieldMapBase.ComputeConfigHash(first), FieldMapBase.ComputeConfigHash(second));
        }

        [TestMethod, TestCategory("L0")]
        public void ComputeConfigHash_SameConfiguration_IsStableAcrossRuns()
        {
            var a = new TreeToTagFieldMapOptions { toSkip = 3, ApplyTo = new() { "Bug" } };
            var b = new TreeToTagFieldMapOptions { toSkip = 3, ApplyTo = new() { "Bug" } };
            Assert.AreEqual(FieldMapBase.ComputeConfigHash(a), FieldMapBase.ComputeConfigHash(b));
        }
    }
}
