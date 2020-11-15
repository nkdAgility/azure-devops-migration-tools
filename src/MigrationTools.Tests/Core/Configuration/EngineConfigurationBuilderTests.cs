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
        public void BuildDefaultTest()
        {
            var ecb = CreateEngine();
            ecb.BuildDefault();
        }

        [TestMethod(), TestCategory("L0")]
        public void BuildFromFileTest()
        {
            HelperCreateDefaultConfigFile();
            var ecb = CreateEngine();
            ecb.BuildFromFile();
        }

        [TestMethod(), TestCategory("L0")]
        public void BuildWorkItemMigrationTest()
        {
            var ecb = CreateEngine();
            ecb.BuildWorkItemMigration();
        }

        [TestMethod, TestCategory("L0")]
        public void TestDeseraliseFromJson()
        {
            HelperCreateDefaultConfigFile();
            EngineConfiguration ec;
            StreamReader sr = new StreamReader("configuration.json");
            string configurationjson = sr.ReadToEnd();
            sr.Close();
            ec = NewtonsoftHelpers.DeserializeObject<EngineConfiguration>(configurationjson);
            Assert.AreEqual(10, ec.FieldMaps.Count);
            Assert.AreEqual(12, ec.Processors.Count);
        }

        [TestMethod, TestCategory("L0")]
        public void TestSeraliseToJson()
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
            EngineConfiguration ec = ecb.BuildDefault();
            string json = NewtonsoftHelpers.SerializeObject(ecb.BuildDefault());
            StreamWriter sw = new StreamWriter("configuration.json");
            sw.WriteLine(json);
            sw.Close();
        }
    }
}