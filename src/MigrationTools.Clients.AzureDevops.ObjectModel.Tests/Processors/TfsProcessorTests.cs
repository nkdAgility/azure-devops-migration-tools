using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.Endpoints;
using MigrationTools.Tests;

namespace MigrationTools.Processors.Tests
{
    public class TfsProcessorTests
    {
        protected ServiceProvider Services = ServiceProviderHelper.GetServices();

        [TestInitialize]
        public void Setup()
        {
        }

        protected static TfsTeamSettingsProcessorOptions GetTfsTeamSettingsProcessorOptions()
        {
            // Tfs To Tfs
            var migrationConfig = new TfsTeamSettingsProcessorOptions()
            {
                Enabled = true,
                MigrateTeamSettings = true,
                UpdateTeamSettings = true,
                PrefixProjectToNodes = false,
                Source = GetTfsEndPointOptions("migrationSource1"),
                Target = GetTfsEndPointOptions("migrationTarget1")
            };
            return migrationConfig;
        }

        protected static TfsSharedQueryProcessorOptions GetTfsSharedQueryProcessorOptions()
        {
            // Tfs To Tfs
            var migrationConfig = new TfsSharedQueryProcessorOptions()
            {
                Enabled = true,
                PrefixProjectToNodes = false,
                Target = GetTfsEndPointOptions("migrationTarget1"),
                Source = GetTfsEndPointOptions("migrationSource1"),
            };
            return migrationConfig;
        }

        protected static TfsEndpointOptions GetTfsEndPointOptions(string project)
        {
            return new TfsTeamSettingsEndpointOptions()
            {
                Organisation = "https://dev.azure.com/nkdagility-preview/",
                Project = project,
                AuthenticationMode = AuthenticationMode.AccessToken,
                AccessToken = TestingConstants.AccessToken,
            };
        }
    }
}