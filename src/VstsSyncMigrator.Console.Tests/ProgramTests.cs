using System.IO;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.Core.Configuration;
using Newtonsoft.Json;

namespace VstsSyncMigrator.Console.Tests
{
    [TestClass]
    public class ProgramTests
    {

        IEngineConfigurationBuilder ecb = new EngineConfigurationBuilder(new NullLogger<EngineConfigurationBuilder>());

        [TestMethod]
        public void TestSeraliseToJson()
        {

            string json = JsonConvert.SerializeObject(ecb.BuildDefault(),
                    new FieldMapConfigJsonConverter(),
                    new ProcessorConfigJsonConverter());
            StreamWriter sw = new StreamWriter("configuration.json");
            sw.WriteLine(json);
            sw.Close();

        }

        [TestMethod]
        public void TestDeseraliseFromJson()
        {
            TestSeraliseToJson();
            EngineConfiguration ec;
            StreamReader sr = new StreamReader("configuration.json");
            string configurationjson = sr.ReadToEnd();
            sr.Close();
            ec = JsonConvert.DeserializeObject<EngineConfiguration>(configurationjson,
                new FieldMapConfigJsonConverter(),
                new ProcessorConfigJsonConverter());
            Assert.AreEqual(10, ec.FieldMaps.Count);
            Assert.AreEqual(13, ec.Processors.Count);
        }
    }
}
