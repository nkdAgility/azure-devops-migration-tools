using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.FieldMaps.AzureDevops.ObjectModel;

namespace MigrationTools.Tests.Tools
{
    [TestClass]
    public class OncePerWorkItemTests
    {
        [TestMethod, TestCategory("L0")]
        public void IsFirst_TrueOncePerWorkItemAndKey()
        {
            var once = new OncePerWorkItem();

            Assert.IsTrue(once.IsFirst("1", "source:Custom.X"));
            Assert.IsFalse(once.IsFirst("1", "source:Custom.X"));
            Assert.IsFalse(once.IsFirst("1", "SOURCE:custom.x"));
            Assert.IsTrue(once.IsFirst("1", "target:Custom.Y"));
        }

        [TestMethod, TestCategory("L0")]
        public void IsFirst_ResetsForTheNextWorkItem()
        {
            var once = new OncePerWorkItem();
            once.IsFirst("1", "source:Custom.X");

            Assert.IsTrue(once.IsFirst("2", "source:Custom.X"));
            Assert.IsTrue(once.IsFirst("1", "source:Custom.X"));
        }
    }
}
