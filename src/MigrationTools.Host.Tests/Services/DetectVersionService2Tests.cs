using MigrationTools.Host.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.Services;
using Serilog;
using Serilog.Events;
using MigrationTools.Host.Tests;
using MigrationTools.Tests;
using MigrationTools.Helpers.Tests;

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
            IDetectVersionService2 dos = new DetectVersionService2(new TelemetryLoggerMock(), new Logger<IDetectVersionService2>(loggerFactory), new FakeMigrationToolVersion());
            Assert.IsNotNull(dos);

        }

    }
}