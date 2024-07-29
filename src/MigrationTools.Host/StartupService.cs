using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MigrationTools.Host.Services;
using Serilog;
using static System.Net.Mime.MediaTypeNames;

namespace MigrationTools.Host
{
    public interface IStartupService
    {
        void RunStartupLogic(string[] args);

        void RunExitLogic();
    }

    internal class StartupService : IStartupService
    {
        private readonly IHostApplicationLifetime _LifeTime;
        private readonly IDetectOnlineService _detectOnlineService;
        private readonly IDetectVersionService2 _detectVersionService;
        private readonly ILogger<StartupService> _logger;
        private readonly ITelemetryLogger _telemetryLogger;
        private static Stopwatch _mainTimer = new Stopwatch();

        public StartupService(IHostApplicationLifetime lifeTime, IDetectOnlineService detectOnlineService, IDetectVersionService2 detectVersionService, ILogger<StartupService> logger, ITelemetryLogger telemetryLogger)
        {
            _LifeTime = lifeTime;
            _detectOnlineService = detectOnlineService;
            _detectVersionService = detectVersionService;
            _logger = logger;
            _telemetryLogger = telemetryLogger;
        }

        public void RunStartupLogic(string[] args)
        {
            ApplicationStartup(args);
            Configure(_LifeTime);
        }

        public void Configure(IHostApplicationLifetime appLifetime)
        {

            appLifetime.ApplicationStarted.Register(() =>
            {
                //_logger.LogInformation("Press Ctrl+C to shut down.");
            });


            appLifetime.ApplicationStopping.Register(() =>
            {
                //_logger.LogInformation("Stopping");
            });
            

            appLifetime.ApplicationStopped.Register(() =>
            {
                RunExitLogic();
            });
        }

        public void RunExitLogic()
        {
           if (Environment.ExitCode <0 )
            {
                _logger.LogError("The application ended with an error code of {ExitCode}", Environment.ExitCode);
            }
            _logger.LogTrace("Application Ending");
            _mainTimer.Stop();
            _telemetryLogger.TrackEvent("ApplicationEnd", null,
                new Dictionary<string, double> {
            { "Application_Elapsed", _mainTimer.ElapsedMilliseconds }
                });
            _telemetryLogger.CloseAndFlush();
            _logger.LogDebug("The application ran in {Application_Elapsed} and finished at {Application_EndTime}", _mainTimer.Elapsed.ToString("c"), DateTime.Now.ToUniversalTime().ToLocalTime());
        }

        private void ApplicationStartup(string[] args)
        {
            _mainTimer.Start();
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;            
            _logger.LogTrace("Application Starting");
        }

        protected void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            _logger.LogError((Exception)e.ExceptionObject, "An Unhandled exception occured.");
            _telemetryLogger.TrackException((Exception)e.ExceptionObject);
            //_logger.LogCloseAndFlush();
            System.Threading.Thread.Sleep(5000);
        }

    }
}