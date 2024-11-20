using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.Services.Common;
using MigrationTools.Options;
using Newtonsoft.Json.Linq;
using Spectre.Console;
using Spectre.Console.Cli;
using Spectre.Console.Json;

namespace MigrationTools.Host.Commands
{
    internal class ConfigurationBuilderCommand : AsyncCommand<ConfigurationBuilderCommandSettings>
    {
        private IServiceProvider _services;
        private readonly ILogger _logger;
        private readonly ITelemetryLogger Telemetery;
        private readonly IHostApplicationLifetime _appLifetime;

        public ConfigurationBuilderCommand(
            IServiceProvider services,
            ILogger<InitMigrationCommand> logger,
            ITelemetryLogger telemetryLogger,
            IHostApplicationLifetime appLifetime)
        {
            _services = services;
            _logger = logger;
            Telemetery = telemetryLogger;
            _appLifetime = appLifetime;
        }

        Layout layout = new Layout("Root");


        public override Task<int> ExecuteAsync(CommandContext context, ConfigurationBuilderCommandSettings settings)
        {
            int _exitCode;

            try
            {
                string configFile = settings.ConfigFile;
                if (string.IsNullOrEmpty(configFile))
                {
                    configFile = "configuration.json";
                }
                _logger.LogInformation("ConfigFile: {configFile}", configFile);

                // Load configuration
                OptionsConfigurationBuilder optionsBuilder = _services.GetRequiredService<OptionsConfigurationBuilder>();
                OptionsConfigurationUpgrader optionsUpgrader = _services.GetRequiredService<OptionsConfigurationUpgrader>();
                optionsUpgrader.UpgradeConfiguration(optionsBuilder, configFile);
                _logger.LogInformation("Configuration loaded & Upgraded to latest...");

                var configurationEditorOptions = new[]
                {
                    "Apply Templates",
                    "Save & Exit",
                    "Exit"
                };

                // Prompt the user to select processors
                bool shouldExit = false;
                while (!shouldExit)
                {
                    Console.Clear();
                    DisplayCurrentOptions(optionsBuilder);

                    var selectedOption = AnsiConsole.Prompt(
                         new SelectionPrompt<string>()
                             .Title("Select a configuration section to edit:")
                             .PageSize(10)
                             .AddChoices(configurationEditorOptions));

                    Console.WriteLine($"Selected option: {selectedOption}");

                    switch (selectedOption)
                    {
                        case "Apply Templates":
                            SelectTemplateToApply(optionsBuilder, configFile);
                            break;
                        case "Save & Exit":
                            SaveOptions(optionsBuilder, configFile);
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
                Telemetery.TrackException(ex, null);
                _logger.LogError(ex, "Unhandled exception!");
                _exitCode = 1;
            }
            finally
            {
                // Stop the application once the work is done
                _appLifetime.StopApplication();
            }
            return Task.FromResult(_exitCode);
        }

        private void DisplayCurrentOptions(OptionsConfigurationBuilder optionsBuilder)
        {
            Console.Clear();
            AnsiConsole.WriteLine("Azure DevOps Migration Tools");
            AnsiConsole.Write(
               new FigletText("Config Builder")
                 .LeftJustified()
                 .Color(Color.Purple));


            // Create the tree
            AnsiConsole.WriteLine();
            AnsiConsole.Write(new Markup("[bold purple]Current Loadout[/]"));
            AnsiConsole.WriteLine();
            AnsiConsole.WriteLine();
            var root = new Tree("MigrationTools");
            // Add some nodes
            var endpoints = root.AddNode("Endpoints");
            foreach (var item in optionsBuilder.GetOptions(OptionItemType.Endpoint))
            {
                endpoints.AddNode($"{item.Option.GetType().Name} ('{item.key}')");
            }
            var processors = root.AddNode("Processors");
            foreach (var item in optionsBuilder.GetOptions(OptionItemType.Processor))
            {
                processors.AddNode($"{item.Option.GetType().Name}");
            }
            var tools = root.AddNode("Tools");
            foreach (var item in optionsBuilder.GetOptions(OptionItemType.Tool))
            {
                tools.AddNode($"{item.Option.GetType().Name}");
            }

            // Render the tree
            AnsiConsole.Write(root);
        }

        private void SaveOptions(OptionsConfigurationBuilder optionsBuilder, string configFile)
        {
            string json = optionsBuilder.Build();
            if (File.Exists(configFile))
            {
                File.Move(configFile, AddSuffixToFileName(configFile, $"-{DateTime.Now.ToString("yyyyMMddHHmmss")}"));
            }
            File.WriteAllText(configFile, json);
            _logger.LogInformation("New {configFile} file has been created", configFile);
            AnsiConsole.Write(new JsonText(json));
        }


        private void SelectTemplateToApply(OptionsConfigurationBuilder optionsBuilder, string configFile)
        {

            bool shouldExit = false;
            while (!shouldExit)
            {
                Console.Clear();
                DisplayCurrentOptions(optionsBuilder);
                var options = new List<string>();
                options.AddRange(Enum.GetNames(typeof(OptionsConfigurationTemplate)));
                options.Add("Save & Exit");
                options.Add("Exit");

                var selectedOption = AnsiConsole.Prompt(
                     new SelectionPrompt<string>()
                         .Title("Select a Template to apply to your config:")
                         .PageSize(10)
                         .AddChoices(options));

                switch (selectedOption)
                {
                    case "Save & Exit":
                        SaveOptions(optionsBuilder, configFile);
                        shouldExit = true;
                        break;
                    case "Exit":
                        shouldExit = true;
                        break;
                    default:
                        OptionsConfigurationTemplate tempopt = (OptionsConfigurationTemplate)Enum.Parse(typeof(OptionsConfigurationTemplate), selectedOption);
                        optionsBuilder.ApplyTemplate(tempopt);
                        break;
                }
            }
        }


        static string AddSuffixToFileName(string filePath, string suffix)
        {
            // Get the directory path
            string directory = Path.GetDirectoryName(filePath);

            // Get the file name without the extension
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);

            // Get the file extension
            string extension = Path.GetExtension(filePath);

            // Combine them to create the new file name
            string newFileName = $"{fileNameWithoutExtension}{suffix}{extension}";

            // Combine the directory with the new file name
            return Path.Combine(directory, newFileName);
        }

    }
}
