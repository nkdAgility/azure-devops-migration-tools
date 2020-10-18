using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.Tests;

namespace MigrationTools.Processors.Tests
{
    [TestClass()]
    public class WorkItemMigrationProcessorTests
    {
        private ServiceProvider Services;

        [TestInitialize]
        public void Setup()
        {
            Services = ServiceProviderHelper.GetWorkItemMigrationProcessor();
        }

        [TestMethod()]
        public void ConfigureTest()
        {
            var y = new WorkItemMigrationProcessorOptions
            {
                Enabled = true
            };
            var x = new WorkItemMigrationProcessor(null, null, null);
            x.Configure(y);

            Assert.IsNotNull(x);
        }

        [TestMethod()]
        public void RunTest()
        {
            var y = new WorkItemMigrationProcessorOptions
            {
                Enabled = true,
                CollapseRevisions = false,
                ReplayRevisions = true,
                WorkItemCreateRetryLimit = 5,
                PrefixProjectToNodes = false
            };
            var x = Services.GetRequiredService<WorkItemMigrationProcessor>();
            x.Configure(y);
            x.Execute();
            Assert.AreEqual(ProcessingStatus.Complete, x.Status);
        }
    }
}