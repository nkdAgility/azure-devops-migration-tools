﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MigrationTools.Services;
using Serilog;
using Serilog.Events;

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
            IDetectVersionService2 dos = new DetectVersionService2(new TelemetryLoggerMock());
            Assert.IsNotNull(dos);

        }

        [TestMethod, TestCategory("L3")]
        public void DetectVersionServiceTest_Update()
        {
            IDetectVersionService2 dos = new DetectVersionService2(new TelemetryLoggerMock());
            dos.UpdateFromSource();

            Assert.IsNotNull(dos);
        }


    }
}