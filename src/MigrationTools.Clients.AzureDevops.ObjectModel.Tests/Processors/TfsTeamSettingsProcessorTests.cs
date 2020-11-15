using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.Endpoints;
using MigrationTools.Tests;

namespace MigrationTools.Processors.Tests
{
    [TestClass()]
    public class TfsTeamSettingsProcessorTests : TfsProcessorTests
    {
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
                PrefixProjectToNodes = false,
                Source = GetTfsWorkItemEndPointOptions("source"),
                Target = GetTfsWorkItemEndPointOptions("target"),
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
                Source = GetTfsWorkItemEndPointOptions("source"),
                Target = GetTfsWorkItemEndPointOptions("target"),
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

        private static TfsEndpointOptions GetTfsWorkItemEndPointOptions(string project)
        {
            return new TfsEndpointOptions()
            {
                Organisation = "https://dev.azure.com/nkdagility-preview/",
                Project = project,
                AuthenticationMode = AuthenticationMode.AccessToken,
                AccessToken = TestingConstants.AccessToken,
            };
        }
    }
}