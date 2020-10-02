using MigrationTools.Configuration;
using System.IO;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace MigrationTools.Configuration.Tests
{
    [TestClass()]
    public class EngineConfigurationBuilderTests
    {

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
            string json = JsonConvert.SerializeObject(ecb.BuildDefault(),
                     new FieldMapConfigJsonConverter(),
                     new ProcessorConfigJsonConverter());
            StreamWriter sw = new StreamWriter("configuration.json");
            sw.WriteLine(json);
            sw.Close();
        }

        [TestMethod()]
        public void BuildFromFileTest()
        {
            HelperCreateDefaultConfigFile();
            var ecb = CreateEngine();
            ecb.BuildFromFile();
        }

        [TestMethod()]
        public void BuildDefaultTest()
        {
            var ecb = CreateEngine();
            ecb.BuildDefault();
        }

        [TestMethod()]
        public void BuildWorkItemMigrationTest()
        {
            var ecb = CreateEngine();
            ecb.BuildWorkItemMigration();
        }

        [TestMethod]
        public void TestSeraliseToJson()
        {

            HelperCreateDefaultConfigFile();

        }

        [TestMethod]
        public void TestDeseraliseFromJson()
        {
            HelperCreateDefaultConfigFile();
             EngineConfiguration ec;
            StreamReader sr = new StreamReader("configuration.json");
            string configurationjson = sr.ReadToEnd();
            sr.Close();
            ec = JsonConvert.DeserializeObject<EngineConfiguration>(configurationjson,
                new FieldMapConfigJsonConverter(),
                new ProcessorConfigJsonConverter());
            Assert.AreEqual(10, ec.FieldMaps.Count);
            Assert.AreEqual(12, ec.Processors.Count);
        }
    }
}