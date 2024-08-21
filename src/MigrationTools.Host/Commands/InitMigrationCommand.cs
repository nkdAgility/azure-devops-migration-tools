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
using MigrationTools._EngineV1.Configuration;
using MigrationTools.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Spectre.Console.Cli;

namespace MigrationTools.Host.Commands
{
    internal class InitMigrationCommand : AsyncCommand<InitMigrationCommandSettings>
    {
        private readonly ILogger _logger;
        private readonly ITelemetryLogger Telemetery;
        private readonly IHostApplicationLifetime _appLifetime;

        public InitMigrationCommand(
            ILogger<InitMigrationCommand> logger,
            ITelemetryLogger telemetryLogger,
            IHostApplicationLifetime appLifetime)
        {
            _logger = logger;
            Telemetery = telemetryLogger;
            _appLifetime = appLifetime;
        }


        public override async Task<int> ExecuteAsync(CommandContext context, InitMigrationCommandSettings settings)
        {
            int _exitCode;
            try
            {
                Telemetery.TrackEvent(new EventTelemetry("InitCommand"));
                string configFile = settings.ConfigFile;
                if (string.IsNullOrEmpty(configFile))
                {
                    configFile = $"configuration-{settings.Options.ToString()}.json";
                }
                _logger.LogInformation("ConfigFile: {configFile}", configFile);
                if (File.Exists(configFile))
                {
                    if (settings.Overwrite)
                    {
                        File.Delete(configFile);
                    }
                    else
                    {
                        _logger.LogCritical($"The config file {configFile} already exists, pick a new name. Or Set --overwrite");
                        Environment.Exit(1);
                    }
                }
                if (!File.Exists(configFile))
                {
                    var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .Build();
                    // get source IOptions bits
                    List<Type> allMigrationTypes = AppDomain.CurrentDomain.GetMigrationToolsTypes().ToList();
                    var allOptions = allMigrationTypes.WithInterface<IOptions>();
                    JObject configJson = new JObject();

                    _logger.LogInformation("Populating config with {Options}", settings.Options.ToString());
                    List<string> optionsToInclude = null;
                    switch (settings.Options)
                    {
                        case OptionsMode.Reference:

                            break;
                        case OptionsMode.Basic:
                             optionsToInclude = new List<string>() { "TfsWorkItemMigrationProcessorOptions", "FieldMappingToolOptions", "FieldLiteralMapOptions" };
                            break;
                        case OptionsMode.WorkItemTracking:
                             optionsToInclude = new List<string>() { "TfsWorkItemMigrationProcessorOptions", "FieldMappingToolOptions", "FieldLiteralMapOptions" };
                            break;
                        default:
                            optionsToInclude = new List<string>() { "TfsWorkItemMigrationProcessorOptions", "FieldMappingToolOptions", "FieldLiteralMapOptions" };
                            break;
                    }

                    if (optionsToInclude != null)
                    {
                        foreach (var item in optionsToInclude)
                        {
                            var item2 = allOptions.FirstOrDefault(x => x.Name == item);
                            configJson = AddOptionToConfig(configuration, configJson, item2);
                        }
                    } else
                    {
                        _logger.LogWarning($"You are adding all of the Options, there may be some that cant be added and will cause an error...");
                        foreach (var item in allOptions)
                        {
                            configJson = AddOptionToConfig(configuration, configJson, item);
                        }
                    }


                    File.WriteAllText(configFile, configJson.ToString(Formatting.Indented));
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

        private JObject AddOptionToConfig(IConfigurationRoot configuration, JObject configJson, Type item)
        {
            IOptions instanceOfOption = (IOptions)Activator.CreateInstance(item);
            bool isCollection = !string.IsNullOrEmpty(instanceOfOption.ConfigurationCollectionPath);
            var section = configuration.GetSection(instanceOfOption.ConfigurationSectionPath);
            section.Bind(instanceOfOption);
            try
            {
                configJson = Options.OptionsManager.AddOptionsToConfiguration(configJson, instanceOfOption, isCollection);
                _logger.LogInformation("Adding Option: {item}", item.Name);
            }
            catch (Exception)
            {

                _logger.LogInformation("FAILED!! Adding Option: {item}", item.FullName);
            }

            return configJson;
        }
    }
}
