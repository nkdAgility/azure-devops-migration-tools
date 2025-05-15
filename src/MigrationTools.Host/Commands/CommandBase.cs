using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MigrationTools.Host.Services;
using MigrationTools.Services;
using OpenTelemetry.Trace;
using Serilog;
using Spectre.Console;
using Spectre.Console.Cli;

namespace MigrationTools.Host.Commands
{
    internal abstract class CommandBase<TSettings> : AsyncCommand<TSettings> where TSettings : CommandSettingsBase
    {
        protected IServiceProvider Services { get; }

        private IMigrationToolVersion _MigrationToolVersion;

        protected IHostApplicationLifetime Lifetime { get; }

        private readonly IDetectOnlineService _detectOnlineService;
        private readonly IDetectVersionService2 _detectVersionService;
        private readonly ILogger<CommandBase<TSettings>> _logger;

        protected ITelemetryLogger TelemetryLogger { get; }
        protected IConfiguration Configuration { get; }
        protected ActivitySource ActivitySource { get; }

        public CommandBase(
            IHostApplicationLifetime appLifetime,
            IServiceProvider services,
            IDetectOnlineService detectOnlineService,
            IDetectVersionService2 detectVersionService,
            ILogger<CommandBase<TSettings>> logger,
            ITelemetryLogger telemetryLogger,
            IMigrationToolVersion migrationToolVersion,
            IConfiguration configuration,
            ActivitySource activitySource)
        {
            _MigrationToolVersion = migrationToolVersion;
            _detectOnlineService = detectOnlineService;
            _detectVersionService = detectVersionService;
            _logger = logger;
            TelemetryLogger = telemetryLogger;
            Lifetime = appLifetime;
            Services = services;
            Configuration = configuration;
            this.ActivitySource = activitySource;

        }

        public Activity CommandActivity { get; private set; }

        public sealed override async Task<int> ExecuteAsync(CommandContext context, TSettings settings)
        {
            using (CommandActivity = ActivitySource.StartActivity(this.GetType().Name))
            {
                CommandActivity.SetTagsFromObject(settings);
                // Disable Telemetry
                if (settings.DisableTelemetry)
                {
                    Log.Debug("Disabling Telemetry {CommandName}", this.GetType().Name);
                    CommandActivity.AddTag("DisableTelemetry", settings.DisableTelemetry);
                    CommandActivity.Stop();
                    ActivitySourceProvider.DisableActivitySource();
                }
                //Enable Debug Trace
                if (settings.DebugTrace)
                {
                    Log.Debug("Enabling Telemetry DebugTrace {CommandName}", this.GetType().Name);
                    ActivitySourceProvider.EnableTelemeteryDebug();
                }

                // Run the command
                Log.Verbose("Starting {CommandName}", this.GetType().Name);
                CommandActivity.AddEvent(new ActivityEvent("Starting"));
                RunStartupLogic(settings);
                try
                {
                    AnsiConsole.Write(new FigletText(context.Name).LeftJustified().Color(Color.Purple));
                    CommandActivity?.AddEvent(new ActivityEvent(context.Name));
                    return await ExecuteInternalAsync(context, settings);
                }
                catch (Exception ex)
                {
                    CommandActivity.RecordException(ex);
                    TelemetryLogger.TrackException(ex, CommandActivity.Tags);
                    _logger.LogError(ex, "Unhandled exception!");
                    return -1;
                }
                finally
                {
                    Lifetime.StopApplication();
                    CommandActivity?.Stop();
                    _logger.LogInformation("Command {CommandName} completed in {Elapsed}", this.GetType().Name, CommandActivity?.Duration);
                    _logger.LogInformation("Check the logs for errors: {LogPath}", LogLocationService.GetLogPath());
                }

            }
        }

        internal virtual Task<int> ExecuteInternalAsync(CommandContext context, TSettings settings)
        {
            return Task.FromResult( 0);
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
                // not online or you have specified not to
                Log.Warning("You are either not online or have chosen `skipVersionCheck`. We will not check for a newer version of the tools.", _detectVersionService.PackageId);
            }
        }


        private void ApplicationStartup(TSettings settings)
        {
            BoilerplateCli.AsciiLogo(_MigrationToolVersion.GetRunningVersion().versionString, Log.Logger);
            BoilerplateCli.TelemetryNote(settings, Log.Logger);
            _logger.LogInformation("Start Time: {StartTime}", DateTime.Now.ToUniversalTime().ToLocalTime());
            _logger.LogInformation("Running with settings: {@settings}", settings);
            _logger.LogInformation("OSVersion: {OSVersion}", Environment.OSVersion.ToString());
            _logger.LogInformation("Version (Assembly): {Version}", _MigrationToolVersion.GetRunningVersion().versionString);
            _logger.LogInformation("Logpath: {LogPath}", LogLocationService.GetLogPath());
        }


    }
}
