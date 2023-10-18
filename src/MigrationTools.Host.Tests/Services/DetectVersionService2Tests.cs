using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.Services;

namespace MigrationTools.Host.Services.Tests
{
    [TestClass]
    public class DetectVersionService2Tests
    {
        [TestMethod, TestCategory("L3")]
        public void DetectVersionServiceTest_Initialise()
        {
            var dos = new DetectVersionService2(new TelemetryLoggerMock());
            Assert.IsNotNull(dos);
        }


    }
}