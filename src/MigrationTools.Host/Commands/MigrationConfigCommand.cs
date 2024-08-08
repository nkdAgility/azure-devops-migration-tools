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
    internal class MigrationConfigCommand : AsyncCommand<MigrationConfigCommandSettings>
    {
        private readonly IEngineConfigurationBuilder _configurationBuilder;
        private readonly ISettingsWriter _settingWriter;
        private readonly ILogger _logger;
        private readonly ITelemetryLogger Telemetery;
        private readonly IHostApplicationLifetime _appLifetime;

        public MigrationConfigCommand(
            IEngineConfigurationBuilder configurationBuilder,
            ISettingsWriter settingsWriter,
            ILogger<InitMigrationCommand> logger,
            ITelemetryLogger telemetryLogger,
            IHostApplicationLifetime appLifetime)
        {
            _configurationBuilder = configurationBuilder;
            _settingWriter = settingsWriter;
            _logger = logger;
            Telemetery = telemetryLogger;
            _appLifetime = appLifetime;
        }


        public override async Task<int> ExecuteAsync(CommandContext context, MigrationConfigCommandSettings settings)
        {
            int _exitCode;
            try
            {
                Telemetery.TrackEvent(new EventTelemetry("MigrationConfigCommand"));
                string configFile = settings.ConfigFile;
                if (string.IsNullOrEmpty(configFile))
                {
                    configFile = "configuration.json";
                }
                _logger.LogInformation("ConfigFile: {configFile}", configFile);
                if (File.Exists(configFile))
                {
                   //
                   throw new Exception("File already exists! We dont yet support edit");
                }
                if (!File.Exists(configFile))
                {
                  
                   // _settingWriter.WriteSettings(config, configFile);
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
