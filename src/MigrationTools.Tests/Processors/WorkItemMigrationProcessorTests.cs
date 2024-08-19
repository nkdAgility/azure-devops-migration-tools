using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.Endpoints;
using MigrationTools.Processors.Infrastructure;
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

        [TestMethod(), TestCategory("L1")]
        [Ignore("Something weired here!")]
        public void WorkItemMigrationProcessorConfigureTest()
        {
            var y = new WorkItemTrackingProcessorOptions
            {
                Enabled = true,
                CollapseRevisions = false,
                ReplayRevisions = true,
                WorkItemCreateRetryLimit = 5,
                SourceName = "Source",
                TargetName = "Target"
            };
            var x = Services.GetRequiredService<WorkItemTrackingProcessor>();
            x.Configure(y);
            Assert.IsNotNull(x);
        }

        [TestMethod(), TestCategory("L2")]
        [Ignore("Something weired here!")]
        public void WorkItemMigrationProcessorRunTest()
        {
            var y = new WorkItemTrackingProcessorOptions
            {
                Enabled = true,
                CollapseRevisions = false,
                ReplayRevisions = true,
                WorkItemCreateRetryLimit = 5,
                SourceName = "Source",
                TargetName = "Target"
            };
            var x = Services.GetRequiredService<WorkItemTrackingProcessor>();
            x.Configure(y);
            x.Execute();
            Assert.AreEqual(ProcessingStatus.Complete, x.Status);
        }
    }
}