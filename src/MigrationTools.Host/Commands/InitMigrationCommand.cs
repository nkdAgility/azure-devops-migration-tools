using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Elmah.Io.Client;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MigrationTools._EngineV1.Configuration;
using MigrationTools.Endpoints.Infrastructure;
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
                    Dictionary<string, string> endpointsToInclude = null;
                    switch (settings.Options)
                    {
                        case OptionsMode.Reference:

                            break;
                        case OptionsMode.Basic:
                             optionsToInclude = new List<string>() { "TfsWorkItemMigrationProcessor", "FieldMappingTool", "FieldLiteralMap" };
                            endpointsToInclude = new Dictionary<string, string> () { { "Source", "TfsTeamProjectEndpoint" }, { "Target", "TfsTeamProjectEndpoint" } };
                            break;
                        case OptionsMode.WorkItemTracking:
                             optionsToInclude = new List<string>() { "TfsWorkItemMigrationProcessor", "FieldMappingTool", "FieldLiteralMap" };
                            endpointsToInclude = new Dictionary<string, string>() { { "Source", "TfsTeamProjectEndpoint" }, { "Target", "TfsTeamProjectEndpoint" } };
                            break;
                            case OptionsMode.PipelineProcessor:
                        default:
                            optionsToInclude = new List<string>() { "AzureDevOpsPipelineProcessor"};
                            endpointsToInclude = new Dictionary<string, string>() { { "Source", "AzureDevOpsEndpoint" }, { "Target", "AzureDevOpsEndpoint" } };
                            break;
                    }

                    if (endpointsToInclude !=null)
                    {
                        foreach (var item in endpointsToInclude)
                        {
                            var item2 = allOptions.WithInterface<IEndpointOptions>().FirstOrDefault(x => x.Name.StartsWith(item.Value));
                            configJson = AddEndpointOptionToConfig(configuration, configJson, item.Key, item2);
                        }
                    } else
                    {
                        _logger.LogWarning($"You are adding all of the EndPoints, there may be some that cant be added and will cause an error...");
                        int epNo = 1;
                        foreach (var item in allOptions.WithInterface<IEndpointOptions>())
                        {
                            configJson = AddEndpointOptionToConfig(configuration, configJson, $"Endpoint{epNo}", item);
                            epNo++;
                        }
                    }

                    if (optionsToInclude != null)
                    {
                        foreach (var item in optionsToInclude)
                        {
                            var item2 = allOptions.FirstOrDefault(x => x.Name.StartsWith(item));
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
                    _logger.LogInformation("New {configFile} file has been created", configFile);
                    _logger.LogInformation(configJson.ToString(Formatting.Indented));

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

        private JObject AddEndpointOptionToConfig(IConfigurationRoot configuration, JObject configJson, string key, Type endpointType)
        {
            IOptions instanceOfOption = (IOptions)Activator.CreateInstance(endpointType);
            var section = configuration.GetSection(instanceOfOption.ConfigurationMetadata.PathToInstance);
            section.Bind(instanceOfOption);
            try
            {
                //instanceOfOption.ConfigurationMetadata.Path = $"MigrationTools:Endpoints:{key}";
                var hardPath = $"MigrationTools:Endpoints:{key}";
                configJson = Options.OptionsManager.AddOptionsToConfiguration(configJson, instanceOfOption, hardPath, true);
                _logger.LogInformation("Adding Option: {item}", endpointType.Name);
            }
            catch (Exception)
            {

                _logger.LogInformation("FAILED!! Adding Option: {item}", endpointType.FullName);
            }

            return configJson;
        }

        private JObject AddOptionToConfig(IConfigurationRoot configuration, JObject configJson, Type item)
        {
            IOptions instanceOfOption = (IOptions)Activator.CreateInstance(item);
            var section = configuration.GetSection(instanceOfOption.ConfigurationMetadata.PathToInstance);
            section.Bind(instanceOfOption);
            try
            {
                configJson = Options.OptionsManager.AddOptionsToConfiguration(configJson, instanceOfOption, false);
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
