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
            if (_detectOnlineService.IsOnline() && !args.Contains("skipVersionCheck"))
            {
                Log.Verbose("Package Management Info:");
                Log.Debug("     IsPackageManagerInstalled: {IsPackageManagerInstalled}", _detectVersionService.IsPackageManagerInstalled);
                Log.Debug("     IsPackageInstalled: {IsPackageInstalled}", _detectVersionService.IsPackageInstalled);
                Log.Debug("     IsUpdateAvailable: {IsUpdateAvailable}", _detectVersionService.IsUpdateAvailable);
                Log.Debug("     IsNewLocalVersionAvailable: {IsNewLocalVersionAvailable}", _detectVersionService.IsNewLocalVersionAvailable);
                Log.Debug("     IsRunningInDebug: {IsRunningInDebug}", _detectVersionService.IsRunningInDebug);
                Log.Verbose("Full version data: ${_detectVersionService}", _detectVersionService);

                Log.Information("Verion Info:");
                Log.Information("     Running: {RunningVersion}", _detectVersionService.RunningVersion);
                Log.Information("     Installed: {InstalledVersion}", _detectVersionService.InstalledVersion);
                Log.Information("     Available: {AvailableVersion}", _detectVersionService.AvailableVersion);
                
               
                if (!_detectVersionService.IsPackageManagerInstalled)
                {
                    Log.Warning("Windows Client: The Windows Package Manager is not installed, we use it to determine if you have the latest version, and to make sure that this application is up to date. You can download and install it from https://aka.ms/getwinget. After which you can call `winget install {PackageId}` from the Windows Terminal to get a manged version of this program.", _detectVersionService.PackageId);
                    Log.Warning("Windows Server: If you are running on Windows Server you can use the experimental version of Winget, or you can still use Chocolatey to manage the install. Install chocolatey from https://chocolatey.org/install and then use `choco install vsts-sync-migrator` to install, and `choco upgrade vsts-sync-migrator` to upgrade to newer versions.", _detectVersionService.PackageId);
                } else
                {
                    if (!_detectVersionService.IsPackageInstalled && !_detectVersionService.IsRunningInDebug)
                    {
                        Log.Information("It looks like this application has been installed from a zip, would you like to use the managed version?");
                        Console.WriteLine("Do you want install the managed version? (y/n)");
                        if (Console.ReadKey().Key == ConsoleKey.Y)
                        {
                            
                            _detectVersionService.UpdateFromSource();
                        }
                    }
                    if (_detectVersionService.IsUpdateAvailable && _detectVersionService.IsPackageInstalled)
                    {
                        Log.Information("It looks like this application has been installed from a zip, would you like to use the managed version from Winget?");
                        Console.WriteLine("Do you want install the managed version? (y/n)");
                        if (Console.ReadKey().Key == ConsoleKey.Y)
                        {
                            _detectVersionService.UpdateFromSource();
                        }
                    }
                    if ((_detectVersionService.IsNewLocalVersionAvailable && _detectVersionService.IsPackageInstalled) && !_detectVersionService.IsRunningInDebug)
                    {
                        Log.Information("It looks like this package ({PackageId}) has been updated locally to version {InstalledVersion} and you are not running the latest version?", _detectVersionService.PackageId, _detectVersionService.InstalledVersion);
                        Console.WriteLine("Do you want to quit and restart? (y/n)");
                        if (Console.ReadKey().Key == ConsoleKey.Y)
                        {
                            Log.Information("Restarting as {CommandLine}", Environment.CommandLine);
                            Process.Start("devopsmigration", string.Join(" ", Environment.GetCommandLineArgs().Skip(1)));
                            Thread.Sleep(2000);
                            Environment.Exit(0);
                        }
                    }
                }
            } else
            {
                /// not online or you have specified not to
                Log.Warning("You are either not online or have chosen `skipVersionCheck`. We will not check for a newer version of the tools.", _detectVersionService.PackageId);
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
            _logger.LogInformation("Version (Assembly): {Version}", version);
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