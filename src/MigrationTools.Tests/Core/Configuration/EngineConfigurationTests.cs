using System.IO;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools._EngineV1.Configuration;
using MigrationTools.Helpers;

namespace MigrationTools.Tests
{
    [TestClass]
    public class EngineConfigurationTests
    {
        private IEngineConfigurationBuilder ecb = new EngineConfigurationBuilder(new NullLogger<EngineConfigurationBuilder>());

        [TestMethod, TestCategory("L2")]
        public void TestSeraliseToJson()
        {
            string json = NewtonsoftHelpers.SerializeObject(ecb.BuildDefault());
            StreamWriter sw = new StreamWriter("configuration.json");
            sw.WriteLine(json);
            sw.Close();
        }

        [TestMethod, TestCategory("L2")]
        public void TestDeseraliseFromJson()
        {
            TestSeraliseToJson();
            StreamReader sr = new StreamReader("configuration.json");
            string configurationjson = sr.ReadToEnd();
            sr.Close();
            EngineConfiguration ec = NewtonsoftHelpers.DeserializeObject<EngineConfiguration>(configurationjson);
            Assert.AreEqual(10, ec.FieldMaps.Count);
            Assert.AreEqual(12, ec.Processors.Count);
        }

        [TestMethod, TestCategory("L2")]
        public void TestSeraliseToJson2()
        {
            string json = NewtonsoftHelpers.SerializeObject(ecb.BuildDefault());
            StreamWriter sw = new StreamWriter("configuration2.json");
            sw.WriteLine(json);
            sw.Close();
        }

        [TestMethod, TestCategory("L2")]
        public void TestDeseraliseFromJson2()
        {
            TestSeraliseToJson2();
            StreamReader sr = new StreamReader("configuration2.json");
            string configurationjson = sr.ReadToEnd();
            sr.Close();
            EngineConfiguration ec = NewtonsoftHelpers.DeserializeObject<EngineConfiguration>(configurationjson);
            Assert.AreEqual(10, ec.FieldMaps.Count);
            Assert.AreEqual(12, ec.Processors.Count);
        }
    }
}