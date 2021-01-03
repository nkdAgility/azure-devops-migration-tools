using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
                SourceName = "Source",
                TargetName = "Target"
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
                SourceName = "Source",
                TargetName = "Target"
            };
            return migrationConfig;
        }
    }
}