using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MigrationTools.Host.Services;
using MigrationTools.Services;
using Serilog;
using Spectre.Console;
using Spectre.Console.Cli;

namespace MigrationTools.Host.Commands
{
    internal abstract class CommandBase<TSettings> : AsyncCommand<TSettings> where TSettings : CommandSettingsBase
    {
        private IServiceProvider _services;
        private IMigrationToolVersion _MigrationToolVersion;
        private readonly IHostApplicationLifetime _LifeTime;
        private readonly IDetectOnlineService _detectOnlineService;
        private readonly IDetectVersionService2 _detectVersionService;
        private readonly ILogger<CommandBase<TSettings>> _logger;
        private readonly ITelemetryLogger _telemetryLogger;
        private static Stopwatch _mainTimer = new Stopwatch();

        public CommandBase(IHostApplicationLifetime appLifetime, IServiceProvider services, IDetectOnlineService detectOnlineService, IDetectVersionService2 detectVersionService, ILogger<CommandBase<TSettings>> logger, ITelemetryLogger telemetryLogger, IMigrationToolVersion migrationToolVersion)
        {
            _services = services;
            _MigrationToolVersion = migrationToolVersion;
            _LifeTime = appLifetime;
            _detectOnlineService = detectOnlineService;
            _detectVersionService = detectVersionService;
            _logger = logger;
            _telemetryLogger = telemetryLogger;
        }

        public override async Task<int> ExecuteAsync(CommandContext context, TSettings settings)
        {
            _mainTimer.Start();
            // Disable Telemetry
            TelemetryConfiguration ai = _services.GetService<TelemetryConfiguration>();
            ai.DisableTelemetry = settings.DisableTelemetry;
            // Run the command
            Log.Debug("Starting {CommandName}", this.GetType().Name);
            _telemetryLogger.TrackEvent(this.GetType().Name);
            RunStartupLogic(settings);
            try
            {
                return await ExecuteInternalAsync(context, settings);
            }
            catch (Exception ex)
            {
                _telemetryLogger.TrackException(ex, null, null);
                _logger.LogError(ex, "Unhandled exception!");
                return 1;
            }
            finally
            {
                _LifeTime.StopApplication();
                _mainTimer.Stop();
                _logger.LogInformation("Command {CommandName} completed in {Elapsed}", this.GetType().Name, _mainTimer.Elapsed);
            }
        }

        internal virtual async Task<int> ExecuteInternalAsync(CommandContext context, TSettings settings)
        {
            // no-op
            return 0;
        }

        public void RunStartupLogic(TSettings settings)
        {
            ApplicationStartup(settings);
            if (!settings.skipVersionCheck && _detectOnlineService.IsOnline())
            {
                _logger.LogTrace("Package Management Info:");
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

                if (_detectVersionService.RunningVersion.Major == 0)
                {
                    Log.Information("Git Info:");
                    Log.Information("     Repo: {GitRepositoryUrl}", ThisAssembly.Git.RepositoryUrl);
                    Log.Information("     Tag: {GitTag}", ThisAssembly.Git.Tag);
                    Log.Information("     Branch: {GitBranch}", ThisAssembly.Git.Branch);
                    Log.Information("     Commits: {GitCommits}", ThisAssembly.Git.Commits);

                }

                if (!_detectVersionService.IsPackageManagerInstalled)
                {
                    Log.Warning("Windows Client: The Windows Package Manager is not installed, we use it to determine if you have the latest version, and to make sure that this application is up to date. You can download and install it from https://aka.ms/getwinget. After which you can call `winget install {PackageId}` from the Windows Terminal to get a manged version of this program.", _detectVersionService.PackageId);
                    Log.Warning("Windows Server: If you are running on Windows Server you can use the experimental version of Winget, or you can still use Chocolatey to manage the install. Install chocolatey from https://chocolatey.org/install and then use `choco install vsts-sync-migrator` to install, and `choco upgrade vsts-sync-migrator` to upgrade to newer versions.", _detectVersionService.PackageId);
                }
                else
                {
                    if (!_detectVersionService.IsRunningInDebug)
                    {
                        if (!_detectVersionService.IsPackageInstalled)
                        {
                            Log.Information("It looks like this application has been installed from a zip, would you like to use the managed version?");
                            Console.WriteLine("Do you want exit and install the managed version? (y/n)");
                            if (Console.ReadKey().Key == ConsoleKey.Y)
                            {
                                Thread.Sleep(2000);
                                Environment.Exit(0);
                            }
                        }
                        if (_detectVersionService.IsUpdateAvailable && _detectVersionService.IsPackageInstalled)
                        {
                            Log.Information("It looks like an updated version is available from Winget, would you like to exit and update?");
                            Console.WriteLine("Do you want to exit and update? (y/n)");
                            if (Console.ReadKey().Key == ConsoleKey.Y)
                            {
                                Thread.Sleep(2000);
                                Environment.Exit(0);
                            }
                        }
                    }
                    else
                    {
                        Log.Information("Running in Debug! No further version checkes.....");
                    }
                }
            }
            else
            {
                /// not online or you have specified not to
                Log.Warning("You are either not online or have chosen `skipVersionCheck`. We will not check for a newer version of the tools.", _detectVersionService.PackageId);
            }
        }


        private void ApplicationStartup( TSettings settings)
        {
            _mainTimer.Start();
            AsciiLogo(_MigrationToolVersion.GetRunningVersion().versionString);
            TelemetryNote(settings);
            _logger.LogInformation("Start Time: {StartTime}", DateTime.Now.ToUniversalTime().ToLocalTime());
            _logger.LogInformation("Running with settings: {@settings}", settings);
            _logger.LogInformation("OSVersion: {OSVersion}", Environment.OSVersion.ToString());
            _logger.LogInformation("Version (Assembly): {Version}", _MigrationToolVersion.GetRunningVersion().versionString);
        }

        private void TelemetryNote(TSettings settings)
        {
            _logger.LogInformation("--------------------------------------");
            _logger.LogInformation("Telemetry Note:");
            if (settings.DisableTelemetry)
            {
                _logger.LogInformation("   Telemetry is disabled by the user.");
            } else
            {
                _logger.LogInformation("   We use Application Insights to collect usage and error information in order to improve the quality of the tools.");
                _logger.LogInformation("   Currently we collect the following anonymous data:");
                _logger.LogInformation("     -Event data: application version, client city/country, hosting type, item count, error count, warning count, elapsed time.");
                _logger.LogInformation("     -Exceptions: application errors and warnings.");
                _logger.LogInformation("     -Dependencies: REST/ObjectModel calls to Azure DevOps to help us understand performance issues.");
                _logger.LogInformation("   This data is tied to a session ID that is generated on each run of the application and shown in the logs. This can help with debugging. If you want to disable telemetry you can run the tool with '--disableTelemetry' on the command prompt.");
                _logger.LogInformation("   Note: Exception data cannot be 100% guaranteed to not leak production data");
            }
            
            _logger.LogInformation("--------------------------------------");
        }

        private void AsciiLogo(string thisVersion)
        {
            AnsiConsole.Write(new FigletText("Azure DevOps").LeftJustified().Color(Color.Purple));
            AnsiConsole.Write(new FigletText("Migration Tools").LeftJustified().Color(Color.Purple));
            var productName = ((AssemblyProductAttribute)Assembly.GetEntryAssembly()
                .GetCustomAttributes(typeof(AssemblyProductAttribute), true)[0]).Product;
            _logger.LogInformation("{productName} ", productName);
            _logger.LogInformation("{thisVersion}", thisVersion);
            var companyName = ((AssemblyCompanyAttribute)Assembly.GetEntryAssembly()
                .GetCustomAttributes(typeof(AssemblyCompanyAttribute), true)[0]).Company;
            _logger.LogInformation("{companyName} ", companyName);
            _logger.LogInformation("===============================================================================");
        }
    }
}
