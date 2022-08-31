using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.Enrichers;
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

        [TestMethod(), TestCategory("L0"), TestCategory("AzureDevOps.ObjectModel")]
        public void GetTfsNodeStructure_WithDifferentAreaPath()
        {
            var nodeStructure = _services.GetRequiredService<TfsNodeStructure>();

            nodeStructure.ApplySettings(new TfsNodeStructureSettings
            {
                SourceProjectName = "SourceProject",
                TargetProjectName = "TargetProject",
                FoundNodes = new Dictionary<string, bool>
                {
                    { @"TargetProject\Area\test\PUL", true }
                }
            });

            var options = new TfsNodeStructureOptions();
            options.SetDefaults();
            options.AreaMaps[@"^SourceProject\\PUL"] = "TargetProject\\test\\PUL";
            nodeStructure.Configure(options);

            const string sourceNodeName = @"SourceProject\PUL";
            const TfsNodeStructureType nodeStructureType = TfsNodeStructureType.Area;

            var newNodeName = nodeStructure.GetNewNodeName(sourceNodeName, nodeStructureType);

            Assert.AreEqual(newNodeName, @"TargetProject\test\PUL");

        }
    }
}