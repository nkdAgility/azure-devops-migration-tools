﻿using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MigrationTools.Processors.Tests
{
    [TestClass()]
    public class AzureDevOpsPipelineProcessorTests : AzureDevOpsProcessorTests
    {
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
                SourceName = "Source",
                TargetName = "Target"
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
                SourceName = "Source",
                TargetName = "Target"
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

        [TestMethod, TestCategory("L3"), TestCategory("AzureDevOps.REST")]
        public void AzureDevOpsPipelineProcessorSelectedBuildDefinitionsTest()
        {
            var config = new AzureDevOpsPipelineProcessorOptions
            {
                BuildPipelines = new List<string> { "Test1", "Test2"},
                MigrateBuildPipelines = true,
                Enabled = true,
                SourceName = "Source",
                TargetName = "Target",
            };
            var processor = Services.GetRequiredService<AzureDevOpsPipelineProcessor>();
            processor.Configure(config);

            processor.Execute();

            Assert.AreEqual(ProcessingStatus.Complete, processor.Status);
        }
    }
}