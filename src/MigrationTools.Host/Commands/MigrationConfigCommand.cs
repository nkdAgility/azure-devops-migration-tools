using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.Services.Common;
using MigrationTools._EngineV1.Configuration;
using MigrationTools._EngineV1.Containers;
using Newtonsoft.Json.Linq;
using Spectre.Console;
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
                    "Source", "Target", "CommonEnrichers",
                    "Processors",
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
                        case "Source":
                            break;
                        case "Target":
                            break;
                        case "CommonEnrichers":
                            break;
                        case "Processors":
                            EditProcessors();
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

        private void EditProcessors()
        {
            Console.Clear();
            bool shouldExit = false;
            while (!shouldExit)
            {
                var options = AppDomain.CurrentDomain.GetMigrationToolsTypes().WithInterface<IProcessor>().Select(c => c.Name.ToString()).OrderBy(c=> c).ToList();
                options.AddRange(new[] { "Save & Exit", "Exit" });
                var selectedOption = AnsiConsole.Prompt(
                     new SelectionPrompt<string>()
                         .Title("Select a configuration section to edit:")
                         .PageSize(10)
                         .AddChoices(options));

                switch (selectedOption)
                {
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

            AppDomain.CurrentDomain.GetMigrationToolsTypes().WithInterface<IProcessor>().ToList().ForEach(x =>
            {
                Console.WriteLine(x.Name);
            });
        }
    }
}
