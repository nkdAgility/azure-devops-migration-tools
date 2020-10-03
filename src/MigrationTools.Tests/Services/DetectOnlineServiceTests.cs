using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MigrationTools.Services.Tests
{
    [TestClass()]
    public class DetectOnlineServiceTests
    {
        [TestMethod()]
        public void DetectOnlineServiceTest()
        {
            var dos = new DetectOnlineService(new TelemetryLoggerMock());
            var result = dos.IsOnline();
            //Assert.IsTrue(result);
        }
    }
}