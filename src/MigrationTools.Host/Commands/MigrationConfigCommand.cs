using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.Services.Common;
using MigrationTools._EngineV1.Configuration;
using MigrationTools._EngineV1.Containers;
using MigrationTools.Options;
using MigrationTools.Processors;
using MigrationTools.Processors.Infrastructure;
using Newtonsoft.Json.Linq;
using Spectre.Console;
using Spectre.Console.Cli;

namespace MigrationTools.Host.Commands
{
    internal class MigrationConfigCommand : AsyncCommand<MigrationConfigCommandSettings>
    {
        private IServiceProvider _services;
        private readonly IEngineConfigurationBuilder _configurationBuilder;
        private readonly ISettingsWriter _settingWriter;
        private readonly ILogger _logger;
        private readonly ITelemetryLogger Telemetery;
        private readonly IHostApplicationLifetime _appLifetime;

        public MigrationConfigCommand(
            IServiceProvider services,
            IEngineConfigurationBuilder configurationBuilder,
            ISettingsWriter settingsWriter,
            ILogger<InitMigrationCommand> logger,
            ITelemetryLogger telemetryLogger,
            IHostApplicationLifetime appLifetime)
        {
            _services = services;
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

                // Load configuration
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .AddJsonFile(configFile, optional: true, reloadOnChange: true)
                    .Build();

                var json = File.ReadAllText(configFile);
                var jsonObj = JObject.Parse(json);


                var configurationEditorOptions = new[]
                {
                    "Templates",
                    "Save & Exit",
                    "Exit"
                };

                // Prompt the user to select processors
                bool shouldExit = false;
                while (!shouldExit)
                {
                    var selectedOption = AnsiConsole.Prompt(
                         new SelectionPrompt<string>()
                             .Title("Select a configuration section to edit:")
                             .PageSize(10)
                             .AddChoices(configurationEditorOptions));

                    Console.WriteLine($"Selected option: {selectedOption}");

                    switch (selectedOption)
                    {
                        case "Apply Templates":
                            SelectTemplateToApply();
                            break;
                        case "Save & Exit":
                            shouldExit = true;
                            break;
                        case "Exit":
                            shouldExit = true;
                            break;
                        default:
                            Console.WriteLine("Unknown Option");
                            break;
                    }
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

        private void SelectTemplateToApply()
        {
            Console.Clear();
            bool shouldExit = false;
            while (!shouldExit)
            {
                var options = new[]
                {
                    "Work Item Migration",
                    "Save & Exit",
                    "Exit"
                };
                options.AddRange(new[] { "Save & Exit", "Exit" });
                var selectedOption = AnsiConsole.Prompt(
                     new SelectionPrompt<string>()
                         .Title("Select a Template to apply to your config:")
                         .PageSize(10)
                         .AddChoices(options));

                switch (selectedOption)
                {
                    case "Work Item Migration":
                        ApplyTemplate("WorkItemMigration");
                        break;
                    case "Save & Exit":
                        shouldExit = true;
                        break;
                    case "Exit":
                        shouldExit = true;
                        break;
                    default:
                        Console.WriteLine($"Selected option: {selectedOption}");
                        break;
                }
            }
        }

        private void ApplyTemplate(string v)
        {
            throw new NotImplementedException();
        }
    }
}
