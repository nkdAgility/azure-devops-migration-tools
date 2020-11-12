using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.Endpoints;
using MigrationTools.Tests;

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
                AuthenticationMode = AuthenticationMode.AccessToken,
                AccessToken = TestingConstants.AccessToken,
            };
        }
    }
}