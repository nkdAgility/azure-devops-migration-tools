using Microsoft.ApplicationInsights;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.Core.Configuration;
using MigrationTools.Core.Configuration.FieldMap;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MigrationTools.Core.Configuration.Tests
{
    [TestClass()]
    public class EngineConfigurationBuilderTests
    {

        private void HelperCreateDefaultConfigFile()
        {
            var ecb = new EngineConfigurationBuilder();
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
            var ecb = new EngineConfigurationBuilder();
            ecb.BuildFromFile();
        }

        [TestMethod()]
        public void BuildDefaultTest()
        {
            var ecb = new EngineConfigurationBuilder();
            ecb.BuildDefault();
        }

        [TestMethod()]
        public void BuildWorkItemMigrationTest()
        {
            var ecb = new EngineConfigurationBuilder();
            ecb.BuildWorkItemMigration();
        }
    }
}