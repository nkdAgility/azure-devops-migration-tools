using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.Enrichers;
using MigrationTools.Processors;
using MigrationTools.Tests;


namespace MigrationTools.ProcessorEnrichers.Tests
{
    [TestClass()]
    public class TfsNodeStructureTests
    {
        private ServiceProvider _services;

        [TestInitialize]
        public void Setup()
        {
            _services = ServiceProviderHelper.GetServices();
        }

        private static TfsNodeStructureOptions GetTfsNodeStructureOptions() => new()
        {
            Enabled = true,
            NodeBasePaths = Array.Empty<string>(),
            PrefixProjectToNodes = false,
            RefName = null
        };

        private static TfsAreaAndIterationProcessorOptions GetTfsAreaAndIterationProcessorOptions() => new()
        {
            PrefixProjectToNodes = false,
            SourceName = "sourceName",
            TargetName = "targetName",
        };


        [TestMethod(), TestCategory("L0"), TestCategory("AzureDevOps.ObjectModel")]
        public void GetTfsNodeStructure_WithDifferentAreaPath()
        {
            var nodeStructure = _services.GetRequiredService<TfsNodeStructure>();

            const string targetStructureName = "Area\\test";
            const string sourceStructureName = "Area";

            nodeStructure.ApplySettings(new TfsNodeStructureSettings
            {
                SourceProjectName = "SourceProject",
                TargetProjectName = "TargetProject",
                FoundNodes = new Dictionary<string, bool>
                {
                    { @"TargetProject\Area\test\PUL", true }
                }
            });

            const string sourceNodeName = @"SourceProject\PUL";
            const TfsNodeStructureType nodeStructureType = TfsNodeStructureType.Area;


            var newNodeName = nodeStructure.GetNewNodeName(sourceNodeName, nodeStructureType, targetStructureName, sourceStructureName);

            Assert.AreEqual(newNodeName, @"TargetProject\test\PUL");

        }
    }
}