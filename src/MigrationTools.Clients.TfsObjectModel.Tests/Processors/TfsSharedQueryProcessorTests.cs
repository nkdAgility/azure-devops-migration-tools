using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MigrationTools.Processors.Tests
{
    [TestClass()]
    public class TfsSharedQueryProcessorTests : TfsProcessorTests
    {
        [TestMethod(DisplayName = "TfsSharedQueryProcessorTests_Incantate"), TestCategory("L0")]
        public void Incantate()
        {
            var x = GetTfsSharedQueryProcessor();
            Assert.IsNotNull(x);
        }

        [TestMethod(DisplayName = "TfsSharedQueryProcessorTests_BasicConfigure"), TestCategory("L0")]
        public void BasicConfigure()
        {
            var y = new TfsSharedQueryProcessorOptions
            {
                Enabled = true,
                PrefixProjectToNodes = false,
                RefName = "fortyTwo",
                SourceName = "Source",
                TargetName = "Target"
            };
            var x = GetTfsSharedQueryProcessor(y);
            Assert.IsNotNull(x);
            Assert.AreEqual("fortyTwo", x.Options.RefName);
        }

        [TestMethod(DisplayName = "TfsSharedQueryProcessorTests_BasicRun"), TestCategory("L0")]
        public void BasicRun()
        {
            var y = new TfsSharedQueryProcessorOptions
            {
                Enabled = true,
                PrefixProjectToNodes = false,
                SourceName = "Source",
                TargetName = "Target"
            };
            var x = GetTfsSharedQueryProcessor(y);
            x.Execute();
            Assert.IsNotNull(x);
        }
    }
}
