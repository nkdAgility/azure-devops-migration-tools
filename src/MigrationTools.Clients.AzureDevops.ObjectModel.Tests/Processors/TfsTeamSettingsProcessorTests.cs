using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools._EngineV1.Configuration;
using MigrationTools.Endpoints;
using MigrationTools.Enrichers;
using MigrationTools.Tests;
using Newtonsoft.Json;

namespace MigrationTools.Processors.Tests
{
    [TestClass()]
    public class TfsTeamSettingsProcessorTests
    {
        private ServiceProvider Services;

        [TestInitialize]
        public void Setup()
        {
            Services = ServiceProviderHelper.GetServices();
        }

        [TestMethod(), TestCategory("L0")]
        public void TfsTeamSettingsProcessorTest()
        {
            var x = Services.GetRequiredService<TfsTeamSettingsProcessor>();
            Assert.IsNotNull(x);
        }

        [TestMethod(), TestCategory("L0")]
        public void ConfigureTest()
        {
            var y = new TfsTeamSettingsProcessorOptions
            {
                Enabled = true,
                MigrateTeamSettings = true,
                UpdateTeamSettings = true,
                PrefixProjectToNodes = false
            };
            var x = Services.GetRequiredService<TfsTeamSettingsProcessor>();
            x.Configure(y);
            Assert.IsNotNull(x);
        }

        [TestMethod(), TestCategory("L0")]
        public void RunTest()
        {
            var y = new TfsTeamSettingsProcessorOptions
            {
                Enabled = true,
                MigrateTeamSettings = true,
                UpdateTeamSettings = true,
                PrefixProjectToNodes = false,
            };
            var x = Services.GetRequiredService<TfsTeamSettingsProcessor>();
            x.Configure(y);
            Assert.IsNotNull(x);
        }

        [TestMethod(), TestCategory("L2")]
        public void TfsTeamSettingsProcessorOptionsJSON()
        {
            var migrationConfig = GetTfsTeamSettingsProcessorOptions();
            string json = JsonConvert.SerializeObject(migrationConfig, Formatting.Indented,
                       new ProcessorConfigJsonConverter(),
                       new JsonConverterForEndpointOptions(),
                       new JsonConverterForEnricherOptions());
            StreamWriter sw = new StreamWriter("../../../../../docs/v2/Reference/JSON/TfsTeamSettingsProcessorOptions.json");
            sw.WriteLine(json);
            sw.Close();
        }

        [TestMethod(), TestCategory("L3")]
        public void TestTfsTeamSettingsProcessorNoEnrichers()
        {
            // Senario 1 Migration from Tfs to Tfs with no Enrichers.
            var migrationConfig = GetTfsTeamSettingsProcessorOptions();
            var processor = Services.GetRequiredService<TfsTeamSettingsProcessor>();
            processor.Configure(migrationConfig);
            processor.Execute();
            Assert.AreEqual(ProcessingStatus.Complete, processor.Status);
        }

        private static TfsTeamSettingsProcessorOptions GetTfsTeamSettingsProcessorOptions()
        {
            // Tfs To Tfs
            var migrationConfig = new TfsTeamSettingsProcessorOptions()
            {
                Enabled = true,
                MigrateTeamSettings = true,
                UpdateTeamSettings = true,

                PrefixProjectToNodes = false,
                Endpoints = new List<IEndpointOptions>()
                    {
                         {GetTfsEndPointOptions(EndpointDirection.Source, "migrationSource1")},
                         {GetTfsEndPointOptions(EndpointDirection.Target, "migrationTarget1")}
                    }
            };
            return migrationConfig;
        }

        private static TfsEndpointOptions GetTfsEndPointOptions(EndpointDirection direction, string project)
        {
            return new TfsTeamSettingsEndpointOptions()
            {
                Direction = direction,
                Organisation = "https://dev.azure.com/nkdagility-preview/",
                Project = project,
                AccessToken = "6i4jyylsadkjanjniaydxnjsi4zsz3qarxhl2y5ngzzffiqdostq",
            };
        }
    }
}