using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools._EngineV1.Configuration;

namespace MigrationTools.Tests
{
    [TestClass]
    public class EngineConfigurationTests
    {
        private EngineConfigurationBuilder ecb = new EngineConfigurationBuilder(new NullLogger<EngineConfigurationBuilder>());

        [TestMethod, TestCategory("L2")]
        public void TestSeraliseToJson()
        {
            var config = ecb.BuildDefault();
            ecb.WriteSettings(config, "configuration.json");
        }

        [TestMethod, TestCategory("L2")]
        public void TestDeseraliseFromJson()
        {
            TestSeraliseToJson();
            var ec = ecb.BuildFromFile("configuration.json");
            Assert.AreEqual(10, ec.FieldMaps.Count);
            Assert.AreEqual(12, ec.Processors.Count);
        }

        [TestMethod, TestCategory("L2")]
        public void TestSeraliseToJson2()
        {
            var config = ecb.BuildDefault();
            ecb.WriteSettings(config, "configuration2.json");
        }

        [TestMethod, TestCategory("L2")]
        public void TestDeseraliseFromJson2()
        {
            TestSeraliseToJson2();
            var ec = ecb.BuildFromFile("configuration2.json");
            Assert.AreEqual(10, ec.FieldMaps.Count);
            Assert.AreEqual(12, ec.Processors.Count);
        }
    }
}