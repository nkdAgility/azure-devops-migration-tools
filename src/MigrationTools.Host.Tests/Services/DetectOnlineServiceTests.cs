using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.Services;

namespace MigrationTools.Host.Services.Tests
{
    [TestClass]
    public class DetectOnlineServiceTests
    {
        [TestMethod, TestCategory("L3")]
        public void DetectOnlineServiceTest()
        {
            var dos = new DetectOnlineService(new TelemetryLoggerMock());
            var result = dos.IsOnline();
            //Assert.IsTrue(result);
        }
    }
}