using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.Services;
using System;
using System.Collections.Generic;
using System.Text;

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
            Assert.IsTrue(result);
        }

    }
}