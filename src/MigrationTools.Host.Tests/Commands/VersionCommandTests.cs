using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.Services;

namespace MigrationTools.Host.Tests.Commands
{
    [TestClass()]
    public class VersionCommandTests
    {
        private IHost host;

        [TestInitialize]
        public void Setup()
        {
            host = MigrationToolHost.CreateDefaultBuilder(new string[] { "version", "--skipVersionCheck" }).Build();
        }

        [TestCleanup]
        public void Cleanup()
        {
            host?.Dispose();
        }

        [TestMethod, TestCategory("L0")]
        public void VersionCommand_ServiceResolution_ShouldSucceed()
        {
            // Verify that all required services can be resolved
            var migrationToolVersion = host.Services.GetRequiredService<IMigrationToolVersion>();
            Assert.IsNotNull(migrationToolVersion);
        }

        [TestMethod, TestCategory("L0")]
        public void VersionCommand_GetRunningVersion_ShouldReturnVersionInfo()
        {
            var migrationToolVersion = host.Services.GetRequiredService<IMigrationToolVersion>();
            var versionInfo = migrationToolVersion.GetRunningVersion();
            
            Assert.IsNotNull(versionInfo);
            Assert.IsNotNull(versionInfo.versionString);
        }
    }
}
