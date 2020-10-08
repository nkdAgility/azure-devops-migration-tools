using System;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.Clients.AzureDevops.ObjectModel;
using MigrationTools.Configuration;

namespace _VstsSyncMigrator.Engine.Tests
{
    [TestClass]
    public class EngineConfigurationTests
    {
        [TestMethod]
        public void EngineConfigurationCreate()
        {
            EngineConfiguration ec = new EngineConfiguration
            {
                LogLevel = Serilog.Events.LogEventLevel.Verbose,
                Source = new TeamProjectConfig() { Project = "DemoProjs", Collection = new Uri("https://sdd2016.visualstudio.com/"), ReflectedWorkItemIDFieldName = "TfsMigrationTool.ReflectedWorkItemId", PersonalAccessToken = "" },
                Target = new TeamProjectConfig() { Project = "DemoProjt", Collection = new Uri("https://sdd2016.visualstudio.com/"), ReflectedWorkItemIDFieldName = "TfsMigrationTool.ReflectedWorkItemId", PersonalAccessToken = "" }
            };
            Assert.IsNotNull(ec);
            Assert.IsNotNull(ec.Source);
            Assert.AreEqual(ec.Source.AsTeamProjectConfig().Project, "DemoProjs");
        }

        [TestMethod]
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