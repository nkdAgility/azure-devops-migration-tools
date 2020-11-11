using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.Endpoints;
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

        [TestMethod(), TestCategory("L0")]
        public void ConfigureTest()
        {
            var y = new WorkItemTrackingProcessorOptions
            {
                Enabled = true
            };
            var x = Services.GetRequiredService<WorkItemTrackingProcessor>();
            x.Configure(y);
            Assert.IsNotNull(x);
        }

        [TestMethod(), TestCategory("L1")]
        public void RunTest()
        {
            var y = new WorkItemTrackingProcessorOptions
            {
                Enabled = true,
                CollapseRevisions = false,
                ReplayRevisions = true,
                WorkItemCreateRetryLimit = 5,
                PrefixProjectToNodes = false,
                Endpoints = new List<IEndpointOptions> {
                    new InMemoryWorkItemEndpointOptions { Direction = EndpointDirection.Source},
                    new InMemoryWorkItemEndpointOptions { Direction = EndpointDirection.Target }
                }
            };
            var x = Services.GetRequiredService<WorkItemTrackingProcessor>();
            x.Configure(y);
            x.Execute();
            Assert.AreEqual(ProcessingStatus.Complete, x.Status);
        }
    }
}