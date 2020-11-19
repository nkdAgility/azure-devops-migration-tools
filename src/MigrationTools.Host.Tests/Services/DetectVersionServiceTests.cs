using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.Services;

namespace MigrationTools.Host.Services.Tests
{
    [TestClass]
    public class DetectVersionServiceTests
    {
        [TestMethod, TestCategory("L3")]
        public void DetectVersionServiceTest()
        {
            var dos = new DetectVersionService(new TelemetryLoggerMock(), new CommandLine.CommonOptions());
            var result = dos.GetLatestVersion();
            Assert.IsNotNull(result);
        }

        [TestMethod, TestCategory("L3")]
        public void DetectVersionServiceSkipVersionCheck()
        {   
            var disableUpdateOption = new CommandLine.CommonOptions();
            disableUpdateOption.SkipUpdateCheck = true;
            var dos = new DetectVersionService(new TelemetryLoggerMock(), disableUpdateOption);
            Assert.IsTrue(dos.SkipUpdateCheck());
        }
    }
}