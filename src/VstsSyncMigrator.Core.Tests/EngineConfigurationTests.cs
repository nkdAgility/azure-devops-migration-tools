using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VstsSyncMigrator.Engine.Configuration;

namespace _VstsSyncMigrator.Engine.Tests
{
    [TestClass]
    public class EngineConfigurationTests
    {
        [TestMethod]
        public void EngineConfigurationCreate()
        {
            EngineConfiguration ec = new EngineConfiguration();
            ec.TelemetryEnableTrace = true;
            ec.Source = new TeamProjectConfig() { Name = "DemoProjs", Collection = new Uri("https://sdd2016.visualstudio.com/") };
            ec.Target = new TeamProjectConfig() { Name = "DemoProjt", Collection = new Uri("https://sdd2016.visualstudio.com/") };
            ec.ReflectedWorkItemIDFieldName = "TfsMigrationTool.ReflectedWorkItemId";
            Assert.IsNotNull(ec);
            Assert.IsNotNull(ec.Source);
            Assert.AreEqual(ec.Source.Name, "DemoProjs");
        }

        [TestMethod]
        public void EngineConfigurationCreateDefault()
        {
            EngineConfiguration ec = EngineConfiguration.GetDefault();
            Assert.IsNotNull(ec);
            Assert.IsNotNull(ec.Source);
            Assert.AreEqual(ec.Source.Name, "DemoProjs");
        }
    }
}
