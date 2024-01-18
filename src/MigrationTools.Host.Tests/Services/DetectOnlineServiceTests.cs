using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.Services;
using Serilog;
using Serilog.Debugging;
using Serilog.Events;

namespace MigrationTools.Host.Services.Tests
{
    [TestClass]
    public class DetectOnlineServiceTests
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
        public void DetectOnlineServiceTest()
        {
            var loggerFactory = new LoggerFactory().AddSerilog();
            var dos = new DetectOnlineService(new TelemetryLoggerMock(), new Logger<DetectOnlineService>(loggerFactory));
            var result = dos.IsOnline();
            //Assert.IsTrue(result);
        }
    }
}