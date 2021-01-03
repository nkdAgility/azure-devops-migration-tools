using System;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools._EngineV1.Configuration;

namespace _VstsSyncMigrator.Engine.Tests
{
    [TestClass]
    public class EngineConfigurationTests
    {
        [TestMethod, TestCategory("L1")]
        public void EngineConfigurationCreate()
        {
            EngineConfiguration ec = new EngineConfiguration
            {
                Source = new TfsTeamProjectConfig() { Project = "DemoProjs", Collection = new Uri("https://sdd2016.visualstudio.com/"), ReflectedWorkItemIDFieldName = "TfsMigrationTool.ReflectedWorkItemId", PersonalAccessToken = "" },
                Target = new TfsTeamProjectConfig() { Project = "DemoProjt", Collection = new Uri("https://sdd2016.visualstudio.com/"), ReflectedWorkItemIDFieldName = "TfsMigrationTool.ReflectedWorkItemId", PersonalAccessToken = "" }
            };
            Assert.IsNotNull(ec);
            Assert.IsNotNull(ec.Source);
            Assert.AreEqual(ec.Source.ToString(), "https://sdd2016.visualstudio.com//DemoProjs");
        }

        [TestMethod, TestCategory("L1")]
        public void EngineConfigurationCreateDefault()
        {
            IEngineConfigurationBuilder ecb = new EngineConfigurationBuilder(new NullLogger<EngineConfigurationBuilder>());
            EngineConfiguration ec = ecb.BuildDefault();
            Assert.IsNotNull(ec);
            Assert.IsNotNull(ec.Source);
            Assert.AreEqual(ec.Source.ToString(), "FakeMigration");
        }
    }
}