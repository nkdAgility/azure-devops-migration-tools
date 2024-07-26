using MigrationTools.Host.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.Services;
using Serilog;
using Serilog.Events;
using MigrationTools.Host.Tests.Services;

namespace MigrationTools.Host.Services.Tests
{
    [TestClass]
    public class DetectVersionService2Tests
    {

        [TestInitialize]
        public void Setup()
        {
            var loggers = new LoggerConfiguration().MinimumLevel.Verbose().Enrich.FromLogContext();
            loggers.WriteTo.Logger(logger => logger
              .WriteTo.Debug(restrictedToMinimumLevel: LogEventLevel.Verbose));
            Log.Logger = loggers.CreateLogger();
            Log.Logger.Information("Logger is initialized");
        }


        [TestMethod, TestCategory("L3")]
        public void DetectVersionServiceTest_Initialise()
        {
            var loggerFactory = new LoggerFactory().AddSerilog();
            IDetectVersionService2 dos = new DetectVersionService2(new TelemetryLoggerMock(), new Logger<IDetectVersionService2>(loggerFactory));
            Assert.IsNotNull(dos);

        }

        [TestMethod(), TestCategory("L0")]
        public void GetRunningVersionTest_Release()
        {
            IMigrationToolVersionInfo versionInfo = new FakeMigrationToolVersionInfo("15.4.4", "15.4.4.0", "v15.4.4");
          var result = DetectVersionService2.GetRunningVersion(versionInfo);
            Assert.AreEqual("15.4.4", result.versionString);
        }

        [TestMethod(), TestCategory("L0")]
        public void GetRunningVersionTest_Preview()
        {
            IMigrationToolVersionInfo versionInfo = new FakeMigrationToolVersionInfo("15.1.5-Preview.3", "15.1.5.3", "v15.1.5-Preview.3");
            var result = DetectVersionService2.GetRunningVersion(versionInfo);
            Assert.AreEqual("15.1.5-Preview.3", result.versionString);
        }

        [TestMethod(), TestCategory("L0")]
        public void GetRunningVersionTest_Canary()
        {
            IMigrationToolVersionInfo versionInfo = new FakeMigrationToolVersionInfo("0.0.0-local+7acec2e6266f5f05b2807264ee8f1db7b94b1949", "0.0.0.0", "v15.1.5-Preview.2-18-g7acec2e");
            var result = DetectVersionService2.GetRunningVersion(versionInfo);
            Assert.AreEqual("15.1.5-Local.2-18-g7acec2e", result.versionString);
        }
    }
}