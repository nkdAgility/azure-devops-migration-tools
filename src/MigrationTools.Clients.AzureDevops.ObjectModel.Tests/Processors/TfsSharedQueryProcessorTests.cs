using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MigrationTools.Processors.Tests
{
    [TestClass()]
    public class TfsSharedQueryProcessorTests : TfsProcessorTests
    {
        [TestMethod(), TestCategory("L0")]
        public void TfsSharedQueryProcessorTest()
        {
            var x = Services.GetRequiredService<TfsSharedQueryProcessor>();
            Assert.IsNotNull(x);
        }

        [TestMethod(), TestCategory("L0")]
        public void ConfigureTest()
        {
            var y = new TfsSharedQueryProcessorOptions
            {
                Enabled = true,
                PrefixProjectToNodes = false
            };
            var x = Services.GetRequiredService<TfsSharedQueryProcessor>();
            x.Configure(y);
            Assert.IsNotNull(x);
        }

        [TestMethod(), TestCategory("L0")]
        public void RunTest()
        {
            var y = new TfsSharedQueryProcessorOptions
            {
                Enabled = true,
                PrefixProjectToNodes = false,
            };
            var x = Services.GetRequiredService<TfsSharedQueryProcessor>();
            x.Configure(y);
            Assert.IsNotNull(x);
        }

        [TestMethod(), TestCategory("L3")]
        public void TestTfsSharedQueryProcessorNoEnrichers()
        {
            // Senario 1 Migration from Tfs to Tfs with no Enrichers.
            var migrationConfig = GetTfsSharedQueryProcessorOptions();
            var processor = Services.GetRequiredService<TfsSharedQueryProcessor>();
            processor.Configure(migrationConfig);
            processor.Execute();
            Assert.AreEqual(ProcessingStatus.Complete, processor.Status);
        }
    }
}