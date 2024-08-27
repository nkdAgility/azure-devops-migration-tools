using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.Processors.Infrastructure;

namespace MigrationTools.Processors.Tests
{
    [TestClass()]
    public class TfsTeamSettingsProcessorTests : TfsProcessorTests
    {
        [TestMethod(), TestCategory("L0")]
        public void TfsTeamSettingsProcessorTest()
        {
            var processor = GetTfsTeamSettingsProcessor();
            Assert.IsNotNull(processor);
        }

        [TestMethod(), TestCategory("L0")]
        public void TfsTeamSettingsProcessorConfigureTest()
        {
            var y = new TfsTeamSettingsProcessorOptions
            {
                Enabled = true,
                MigrateTeamSettings = true,
                UpdateTeamSettings = true,
                PrefixProjectToNodes = false,
                SourceName = "TfsTeamSettingsSource",
                TargetName = "TfsTeamSettingsTarget"
            };
            var x = GetTfsTeamSettingsProcessor(y);
            Assert.IsNotNull(x);
            Assert.AreEqual(x.Options.SourceName, "TfsTeamSettingsSource");
            Assert.IsNotNull(x.Source);
        }

        [TestMethod(), TestCategory("L3")]
        public void TfsTeamSettingsProcessorNoEnrichersTest()
        {
            // Senario 1 Migration from Tfs to Tfs with no Enrichers.
            var processor = GetTfsTeamSettingsProcessor();
            processor.Execute();
            Assert.AreEqual(ProcessingStatus.Complete, processor.Status);
        }
    }
}