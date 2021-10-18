﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Serilog.Events;
using Serilog.Sinks.InMemory;

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

            Func<LogEvent, bool> migrationMessageFilter = (LogEvent le) =>
                le.MessageTemplate.Text == "{ObjectsToBeMigrated} of {TotalObjects} source {DefinitionType}(s) are going to be migrated.." &&
                    GetValueFromProperty(le.Properties["DefinitionType"]).ToString() == nameof(DataContracts.Pipelines.BuildDefinition) &&
                    GetValueFromProperty(le.Properties["ObjectsToBeMigrated"]).ToString() == "0" &&
                    GetValueFromProperty(le.Properties["TotalObjects"]).ToString() == "0";
            Func<LogEvent, bool> configuredMessageFilter = (LogEvent le) =>
                le.MessageTemplate.Text == "Configured {Definition} definitions: {DefinitionList}" &&
                    GetValueFromProperty(le.Properties["Definition"]).ToString() == nameof(DataContracts.Pipelines.BuildDefinition) &&
                    GetValueFromProperty(le.Properties["DefinitionList"]).ToString() == "Test1;Test2";

            Assert.AreEqual(1, InMemorySink.Instance.LogEvents.Count(migrationMessageFilter));
            Assert.AreEqual(2, InMemorySink.Instance.LogEvents.Count(configuredMessageFilter));
        }

        [TestMethod, TestCategory("L3"), TestCategory("AzureDevOps.REST")]
        public void AzureDevOpsPipelineProcessorSelectedReleaseDefinitionsTest()
        {
            var config = new AzureDevOpsPipelineProcessorOptions
            {
                ReleasePipelines = new List<string> { "Test1", "New release pipeline", "Test3" },
                MigrateReleasePipelines = true,
                Enabled = true,
                SourceName = "Source",
                TargetName = "Target",
            };
            var processor = Services.GetRequiredService<AzureDevOpsPipelineProcessor>();
            processor.Configure(config);

            processor.Execute();

            Assert.AreEqual(ProcessingStatus.Complete, processor.Status);

            Func<LogEvent, bool> migrationMessageFilter = (LogEvent le) =>
                le.MessageTemplate.Text == "{ObjectsToBeMigrated} of {TotalObjects} source {DefinitionType}(s) are going to be migrated.." &&
                    GetValueFromProperty(le.Properties["DefinitionType"]).ToString() == nameof(DataContracts.Pipelines.ReleaseDefinition) &&
                    GetValueFromProperty(le.Properties["ObjectsToBeMigrated"]).ToString() == "0" &&
                    GetValueFromProperty(le.Properties["TotalObjects"]).ToString() == "1";
            Func<LogEvent, bool> configuredMessageFilter = (LogEvent le) =>
                le.MessageTemplate.Text == "Configured {Definition} definitions: {DefinitionList}" &&
                    GetValueFromProperty(le.Properties["Definition"]).ToString() == nameof(DataContracts.Pipelines.ReleaseDefinition) &&
                    GetValueFromProperty(le.Properties["DefinitionList"]).ToString() == "Test1;New release pipeline;Test3";

            Assert.AreEqual(1, InMemorySink.Instance.LogEvents.Count(migrationMessageFilter));
            Assert.AreEqual(2, InMemorySink.Instance.LogEvents.Count(configuredMessageFilter));         
        }
    }
}