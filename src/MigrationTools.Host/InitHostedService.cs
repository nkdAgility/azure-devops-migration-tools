using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MigrationTools._EngineV1.Configuration;
using MigrationTools.Host.CommandLine;

namespace MigrationTools.Host
{
    public class InitHostedService : IHostedService
    {
        private readonly IEngineConfigurationBuilder _configurationBuilder;
        private readonly ISettingsWriter _settingWriter;
        private readonly InitOptions _initOptions;
        private readonly ILogger _logger;
        private readonly ITelemetryLogger _telemetryLogger;
        private readonly IHostApplicationLifetime _appLifetime;
        private int? _exitCode;

        public InitHostedService(
            IEngineConfigurationBuilder configurationBuilder,
            ISettingsWriter settingsWriter,
            IOptions<InitOptions> initOptions,
            ILogger<InitHostedService> logger,
            ITelemetryLogger telemetryLogger,
            IHostApplicationLifetime appLifetime)
        {
            _configurationBuilder = configurationBuilder;
            _settingWriter = settingsWriter;
            _initOptions = initOptions.Value;
            _logger = logger;
            _telemetryLogger = telemetryLogger;
            _appLifetime = appLifetime;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _appLifetime.ApplicationStarted.Register(() =>
            {
                Task.Run(() =>
                {
                    try
                    {
                        _telemetryLogger.TrackEvent(new EventTelemetry("InitCommand"));
                        string configFile = _initOptions.ConfigFile;
                        if (string.IsNullOrEmpty(configFile))
                        {
                            configFile = "configuration.json";
                        }
                        _logger.LogInformation("ConfigFile: {configFile}", configFile);
                        if (File.Exists(configFile))
                        {
                            _logger.LogInformation("Deleting old configuration.json reference file");
                            File.Delete(configFile);
                        }
                        if (!File.Exists(configFile))
                        {
                            _logger.LogInformation("Populating config with {Options}", _initOptions.Options.ToString());
                            EngineConfiguration config;
                            switch (_initOptions.Options)
                            {
                                case OptionsMode.Reference:
                                    config = _configurationBuilder.BuildReference();
                                    break;
                                case OptionsMode.Basic:
                                    config = _configurationBuilder.BuildGettingStarted();
                                    break;

                                case OptionsMode.WorkItemTracking:
                                    config = _configurationBuilder.BuildWorkItemMigration();
                                    break;

                                case OptionsMode.Fullv2:
                                    config = _configurationBuilder.BuildDefault2();
                                    break;

                                case OptionsMode.WorkItemTrackingv2:
                                    config = _configurationBuilder.BuildWorkItemMigration2();
                                    break;

                                default:
                                    config = _configurationBuilder.BuildGettingStarted();
                                    break;
                            }
                            _settingWriter.WriteSettings(config, configFile);
                            _logger.LogInformation($"New {configFile} file has been created");
                        }
                        _exitCode = 0;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Unhandled exception!");
                        _exitCode = 1;
                    }
                    finally
                    {
                        // Stop the application once the work is done
                        _appLifetime.StopApplication();
                    }
                });
            });
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogDebug($"Exiting with return code: {_exitCode}");
            if (_exitCode.HasValue)
            {
                Environment.ExitCode = _exitCode.Value;
            }
            return Task.CompletedTask;
        }
    }
}