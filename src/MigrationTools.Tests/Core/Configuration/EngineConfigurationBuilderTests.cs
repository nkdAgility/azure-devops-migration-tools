using System.IO;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools._EngineV1.Configuration;
using MigrationTools.Helpers;

namespace MigrationTools.Configuration.Tests
{
    [TestClass()]
    public class EngineConfigurationBuilderTests
    {
        [TestMethod(), TestCategory("L0")]
        public void EngineConfigurationBuilderBuildDefaultTest()
        {
            var ecb = CreateEngine();
            ecb.BuildDefault();
        }

        [TestMethod(), TestCategory("L0")]
        public void EngineConfigurationBuilderBuildFromFileTest()
        {
            HelperCreateDefaultConfigFile();
            var ecb = CreateEngine();
            ecb.BuildFromFile();
        }

        [TestMethod(), TestCategory("L0")]
        public void EngineConfigurationBuilderBuildWorkItemMigrationTest()
        {
            var ecb = CreateEngine();
            ecb.BuildWorkItemMigration();
        }

        [TestMethod, TestCategory("L0")]
        public void EngineConfigurationBuilderDeseraliseFromJsonTest()
        {
            HelperCreateDefaultConfigFile();
            var ecb = CreateEngine();
            EngineConfiguration ec = ecb.BuildFromFile("configuration.json");
            Assert.AreEqual(10, ec.FieldMaps.Count);
            Assert.AreEqual(12, ec.Processors.Count);
        }

        [TestMethod, TestCategory("L0")]
        public void EngineConfigurationBuilderSeraliseToJsonTest()
        {
            HelperCreateDefaultConfigFile();
        }

        private EngineConfigurationBuilder CreateEngine()
        {
            var logger = new NullLogger<EngineConfigurationBuilder>();
            var ecb = new EngineConfigurationBuilder(logger);
            return ecb;
        }

        private void HelperCreateDefaultConfigFile()
        {
            var ecb = CreateEngine();
            var config = ecb.BuildDefault();
            ecb.WriteSettings(config, "configuration.json");
        }
    }
}