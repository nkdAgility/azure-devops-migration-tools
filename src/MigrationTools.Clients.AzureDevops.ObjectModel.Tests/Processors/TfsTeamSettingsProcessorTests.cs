using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
    }
}