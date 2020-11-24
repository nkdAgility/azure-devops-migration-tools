using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.Tests;

namespace MigrationTools.Processors.Tests
{
    [TestClass()]
    public class AzureDevOpsPipelineProcessorTests : AzureDevOpsProcessorTests
    {
        public ServiceProvider Services { get; private set; }

        [TestInitialize]
        public void Setup()
        {
            Services = ServiceProviderHelper.GetServices();
        }

        [TestMethod(), TestCategory("L0"), TestCategory("AzureDevOps.REST")]
        public void AzureDevOpsPipelineProcessorTest()
        {
            var x = Services.GetRequiredService<AzureDevOpsPipelineProcessor>();
            Assert.IsNotNull(x);
        }

        [TestMethod(), TestCategory("L0"), TestCategory("AzureDevOps.REST")]
        public void AzureDevOpsPipelineProcessorConfigureTest()
        {
            var y = new AzureDevOpsPipelineProcessorOptions
            {
                Enabled = true,
                Source = GetAzureDevOpsEndpointOptions("migrationSource1"),
                Target = GetAzureDevOpsEndpointOptions("migrationTarget1")
            };
            var x = Services.GetRequiredService<AzureDevOpsPipelineProcessor>();
            x.Configure(y);
            Assert.IsNotNull(x);
        }

        [TestMethod(), TestCategory("L0"), TestCategory("AzureDevOps.REST")]
        public void AzureDevOpsPipelineProcessorRunTest()
        {
            var y = new AzureDevOpsPipelineProcessorOptions
            {
                Enabled = true,
                Source = GetAzureDevOpsEndpointOptions("migrationSource1"),
                Target = GetAzureDevOpsEndpointOptions("migrationTarget1")
            };
            var x = Services.GetRequiredService<AzureDevOpsPipelineProcessor>();
            x.Configure(y);
            Assert.IsNotNull(x);
        }

        [TestMethod(), TestCategory("L3"), TestCategory("AzureDevOps.REST")]
        public void AzureDevOpsPipelineProcessorNoEnrichersTest()
        {
            // Senario 1 Migration from Tfs to Tfs with no Enrichers.
            var migrationConfig = GetAzureDevOpsPipelineProcessorOptions();
            var processor = Services.GetRequiredService<AzureDevOpsPipelineProcessor>();
            processor.Configure(migrationConfig);
            processor.Execute();
            Assert.AreEqual(ProcessingStatus.Complete, processor.Status);
        }
    }
}