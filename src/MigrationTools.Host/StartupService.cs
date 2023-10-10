using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MigrationTools.Host.Services;

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
        private readonly IDetectVersionService _detectVersionService;
        private readonly ILogger<StartupService> _logger;
        private readonly ITelemetryLogger _telemetryLogger;
        private static Stopwatch _mainTimer = new Stopwatch();

        public StartupService(IHostApplicationLifetime lifeTime, IDetectOnlineService detectOnlineService, IDetectVersionService detectVersionService, ILogger<StartupService> logger, ITelemetryLogger telemetryLogger)
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
            if (_detectOnlineService.IsOnline())
            {
                Version latestVersion = _detectVersionService.GetLatestVersion();

                _logger.LogInformation($"Latest version detected as {{{nameof(latestVersion)}}}", latestVersion);
                var version = Assembly.GetEntryAssembly().GetName().Version;
                if (latestVersion > version && args.Contains("skipVersionCheck"))
                {
                    _logger.LogWarning("You are currently running version {Version} and a newer version ({LatestVersion}) is available. You should update now using Winget command 'winget nkdAgility.AzureDevOpsMigrationTools' from the Windows Terminal.", version, latestVersion);
#if !DEBUG
                    Console.WriteLine("Do you want to continue? (y/n)");
                    if (Console.ReadKey().Key != ConsoleKey.Y)
                    {
                        _logger.LogWarning("User aborted to update version");
                        throw new Exception("User Abort");
                    }
#endif
                }
            }
        }

        public void Configure(IHostApplicationLifetime appLifetime)
        {
            appLifetime.ApplicationStarted.Register(() =>
            {
                _logger.LogInformation("Press Ctrl+C to shut down.");
            });

            appLifetime.ApplicationStopped.Register(() =>
            {
                RunExitLogic();
            });
        }

        public void RunExitLogic()
        {
            _logger.LogInformation("Application Ending");
            _mainTimer.Stop();
            _telemetryLogger.TrackEvent("ApplicationEnd", null,
                new Dictionary<string, double> {
            { "Application_Elapsed", _mainTimer.ElapsedMilliseconds }
                });
            _telemetryLogger.CloseAndFlush();
            _logger.LogInformation("The application ran in {Application_Elapsed} and finished at {Application_EndTime}", _mainTimer.Elapsed.ToString("c"), DateTime.Now.ToUniversalTime().ToLocalTime());
        }

        private void ApplicationStartup(string[] args)
        {
            _mainTimer.Start();
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            var version = Assembly.GetEntryAssembly().GetName().Version;
            _logger.LogInformation("Application Starting");
            AsciiLogo(version);
            TelemetryNote();
            _logger.LogInformation("Start Time: {StartTime}", DateTime.Now.ToUniversalTime().ToLocalTime());
            _logger.LogInformation("Running with args: {@Args}", args);
            _logger.LogInformation("OSVersion: {OSVersion}", Environment.OSVersion.ToString());
            _logger.LogInformation("Version: {Version}", version);
        }

        protected void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            _logger.LogError((Exception)e.ExceptionObject, "An Unhandled exception occured.");
            //_logger.LogCloseAndFlush();
            System.Threading.Thread.Sleep(5000);
        }

        private void TelemetryNote()
        {
            _logger.LogInformation("Telemetry Note:");
            _logger.LogInformation("   We use Application Insights to collect usage and error information in order to improve the quality of the tools.");
            _logger.LogInformation("   Currently we collect the following anonymous data:");
            _logger.LogInformation("     -Event data: application version, client city/country, hosting type, item count, error count, warning count, elapsed time.");
            _logger.LogInformation("     -Exceptions: application errors and warnings.");
            _logger.LogInformation("     -Dependencies: REST/ObjectModel calls to Azure DevOps to help us understand performance issues.");
            _logger.LogInformation("   This data is tied to a session ID that is generated on each run of the application and shown in the logs. This can help with debugging. If you want to disable telemetry you can run the tool with '--disableTelemetry true' on the command prompt.");
            _logger.LogInformation("   Note: Exception data cannot be 100% guaranteed to not leak production data");
            _logger.LogInformation("--------------------------------------");
        }

        private void AsciiLogo(Version thisVersion)
        {
            _logger.LogInformation("                                      &@&                                      ");
            _logger.LogInformation("                                   @@(((((@                                    ");
            _logger.LogInformation("                                  @(((((((((@                                  ");
            _logger.LogInformation("                                @(((((((((((((&                                ");
            _logger.LogInformation("                              ##((((((@ @((((((@@                              ");
            _logger.LogInformation("                             @((((((@     @((((((&                             ");
            _logger.LogInformation("                            @(((((#        @((((((@                            ");
            _logger.LogInformation("                           &(((((&           &(((((@                           ");
            _logger.LogInformation("                          @(((((&             &(((((@                          ");
            _logger.LogInformation("                          &(((((@#&@((.((&@@@(#(((((@                          ");
            _logger.LogInformation("                         #((((#..................#@((&                         ");
            _logger.LogInformation("                       &@(((((&......................(@                        ");
            _logger.LogInformation("                     @.(&((((&...&&        &@&..........&@                     ");
            _logger.LogInformation("                   @...@(((((@                   @#.......((                   ");
            _logger.LogInformation("                 &.....@(((((@                   @((@.......&                  ");
            _logger.LogInformation("                @......@(((((                    #((((&.......&                ");
            _logger.LogInformation("               #.....( &(((((         @@@        ((((((@@......@               ");
            _logger.LogInformation("              &.....@  @(((&@@#(((((((((((((((((#@(((((&  ......@              ");
            _logger.LogInformation("             @.....@  &@&((((((((((((((((((((((((@(((((@#  ......@             ");
            _logger.LogInformation("            @.....&@(((((((((((((((&&@@@@@(((((@((((#(((#@(....&               ");
            _logger.LogInformation("            @.....&((((((((&@@&                 @(((((@(((((((@...#            ");
            _logger.LogInformation("            &....((((((@@(((((@                &@(((((@&((((((((#&&            ");
            _logger.LogInformation("           @(....&((@    @(((((@               @(((((@    @(((((((##           ");
            _logger.LogInformation("         @(#(....&        &(((((@             @(((((&       &@(((((((&         ");
            _logger.LogInformation("       &@(((&.....        @((((((&           @(((((       &.(&((((((@          ");
            _logger.LogInformation("      @(((((@.....&        (((((@        &@(((((&         @....@((((((@        ");
            _logger.LogInformation("     @(((((#@.....(          &(((((@&     ##(((((&         @.....@@((((((@     ");
            _logger.LogInformation("   (&(((((@  &.....@&         @((((((@   @((((((@         @......   @(((((@    ");
            _logger.LogInformation("   &(((((@    @.....#&         @#((((((@((((((#          @......&    @(((((@   ");
            _logger.LogInformation("  @(((((@      &......&          @(((((((@#((@         &@......       @(((((@  ");
            _logger.LogInformation(" @(((((@        @......@&        @@@(((((((&@&        @......(         #(((((@ ");
            _logger.LogInformation(" #((((&           &.......@  &@&(((((@#((((((((@@& &@.......@          ((((&   ");
            _logger.LogInformation("&(((((@@           @(....&@#((((((((((@ @(((((((#@........@            &@(((((@");
            _logger.LogInformation("&(((((((((((((((((((((((((((((((((&@@@@@@@@@&...........@(((((((((((((((((((((@");
            _logger.LogInformation("@(((((((((((((((((((((((((((((&@(....................@#((((((((((((((((((((((#@");
            _logger.LogInformation("      @((((((((((((((&@&  &&...................@   @@#((((((((((((((#@@        ");
            _logger.LogInformation("                                                                               ");
            _logger.LogInformation("===============================================================================");
            var productName = ((AssemblyProductAttribute)Assembly.GetEntryAssembly()
                .GetCustomAttributes(typeof(AssemblyProductAttribute), true)[0]).Product;
            _logger.LogInformation("{productName} ", productName);
            _logger.LogInformation("v{thisVersion}", thisVersion);
            var companyName = ((AssemblyCompanyAttribute)Assembly.GetEntryAssembly()
                .GetCustomAttributes(typeof(AssemblyCompanyAttribute), true)[0]).Company;
            _logger.LogInformation("{companyName} ", companyName);
            _logger.LogInformation("===============================================================================");
        }
    }
}