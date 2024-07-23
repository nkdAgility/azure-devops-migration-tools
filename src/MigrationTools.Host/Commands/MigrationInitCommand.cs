using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MigrationTools._EngineV1.Configuration;
using Spectre.Console.Cli;

namespace MigrationTools.Host.Commands
{
    internal class MigrationInitCommand : AsyncCommand<MigrationInitCommandSettings>
    {
        private readonly IEngineConfigurationBuilder _configurationBuilder;
        private readonly ISettingsWriter _settingWriter;
        private readonly ILogger _logger;
        private readonly ITelemetryLogger Telemetery;
        private readonly IHostApplicationLifetime _appLifetime;

        public MigrationInitCommand(
            IEngineConfigurationBuilder configurationBuilder,
            ISettingsWriter settingsWriter,
            ILogger<MigrationInitCommand> logger,
            ITelemetryLogger telemetryLogger,
            IHostApplicationLifetime appLifetime)
        {
            _configurationBuilder = configurationBuilder;
            _settingWriter = settingsWriter;
            _logger = logger;
            Telemetery = telemetryLogger;
            _appLifetime = appLifetime;
        }


        public override async Task<int> ExecuteAsync(CommandContext context, MigrationInitCommandSettings settings)
        {
            int _exitCode;
            try
            {
                Telemetery.TrackEvent(new EventTelemetry("InitCommand"));
                string configFile = settings.ConfigFile;
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
                    _logger.LogInformation("Populating config with {Options}", settings.Options.ToString());
                    EngineConfiguration config;
                    switch (settings.Options)
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
                Telemetery.TrackException(ex, null, null);
                _logger.LogError(ex, "Unhandled exception!");
                _exitCode = 1;
            }
            finally
            {
                // Stop the application once the work is done
                _appLifetime.StopApplication();
            }
            return _exitCode;
        }
    }
}
