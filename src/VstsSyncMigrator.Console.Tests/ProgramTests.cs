﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System.IO;
using MigrationTools.Core.Configuration.FieldMap;
using MigrationTools.Core.Configuration;

namespace VstsSyncMigrator.Console.Tests
{
    [TestClass]
    public class ProgramTests
    {
        [TestMethod]
        public void TestSeraliseToJson()
        {
            string json = JsonConvert.SerializeObject(EngineConfiguration.GetDefault(),
                    new FieldMapConfigJsonConverter(),
                    new ProcessorConfigJsonConverter());
            StreamWriter sw = new StreamWriter("configuration.json");
            sw.WriteLine(json);
            sw.Close();

        }

        [TestMethod]
        public void TestDeseraliseFromJson()
        {
            EngineConfiguration ec;
            StreamReader sr = new StreamReader("configuration.json");
            string configurationjson = sr.ReadToEnd();
            sr.Close();
            ec = JsonConvert.DeserializeObject<EngineConfiguration>(configurationjson,
                new FieldMapConfigJsonConverter(),
                new ProcessorConfigJsonConverter());

        }
    }
}
