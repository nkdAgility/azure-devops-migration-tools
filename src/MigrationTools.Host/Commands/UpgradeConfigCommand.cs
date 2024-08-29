﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
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
using NuGet.Packaging;
using Spectre.Console;
using Spectre.Console.Cli;

namespace MigrationTools.Host.Commands
{
    internal class UpgradeConfigCommand : AsyncCommand<UpgradeConfigCommandSettings>
    {
        private readonly IConfiguration configuration;

        public IServiceProvider Services { get; }

        private readonly ILogger _logger;
        private readonly ITelemetryLogger Telemetery;
        private readonly IHostApplicationLifetime _appLifetime;

        private static Dictionary<string, string> classNameChangeLog = new Dictionary<string, string>();

        public UpgradeConfigCommand(
            IConfiguration configuration,
            IServiceProvider services,
            ILogger<InitMigrationCommand> logger,
            ITelemetryLogger telemetryLogger,
            IHostApplicationLifetime appLifetime)
        {
            this.configuration = configuration;
            Services = services;
            _logger = logger;
            Telemetery = telemetryLogger;
            _appLifetime = appLifetime;
        }

        public override async Task<int> ExecuteAsync(CommandContext context, UpgradeConfigCommandSettings settings)
        {
            int _exitCode;

            try
            {
                Telemetery.TrackEvent(new EventTelemetry("UpgradeConfigCommand"));
                string configFile = settings.ConfigFile;
                if (string.IsNullOrEmpty(configFile))
                {
                    configFile = "configuration.json";
                }
                _logger.LogInformation("ConfigFile: {configFile}", configFile);

                //// Load configuration
                //var configuration = new ConfigurationBuilder()
                //    .SetBasePath(Directory.GetCurrentDirectory())
                //    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                //    .AddJsonFile(configFile, optional: true, reloadOnChange: true)
                //    .Build();

                classNameChangeLog.Add("WorkItemMigrationContext", "TfsWorkItemMigrationProcessor");
                classNameChangeLog.Add("TfsTeamProjectConfig", "TfsTeamProjectEndpoint");
                classNameChangeLog.Add("WorkItemGitRepoMappingTool", "TfsGitRepositoryTool");
                classNameChangeLog.Add("WorkItemFieldMappingTool", "FieldMappingTool");

                OptionsConfiguration optionsBuilder = Services.GetService<OptionsConfiguration>();

                switch (VersionOptions.ConfigureOptions.GetMigrationConfigVersion(configuration).schema)
                {
                    case MigrationConfigSchema.v1:
                    case MigrationConfigSchema.v150:
                        // ChangeSetMappingFile
                        optionsBuilder.AddOption(ParseV1TfsChangeSetMappingToolOptions(configuration));
                        optionsBuilder.AddOption(ParseV1TfsGitRepoMappingOptions(configuration));
                        optionsBuilder.AddOption(ParseV1FieldMaps(configuration));
                        optionsBuilder.AddOption(ParseSectionCollectionWithTypePropertyNameToList(configuration, "Processors", "$type"));
                        optionsBuilder.AddOption(ParseSectionCollectionWithTypePropertyNameToList(configuration, "CommonEnrichersConfig", "$type"));
                        if (!IsSectionNullOrEmpty(configuration.GetSection("Source")) || !IsSectionNullOrEmpty(configuration.GetSection("Target")))
                        {
                            optionsBuilder.AddOption(ParseSectionWithTypePropertyNameToOptions(configuration, "Source", "$type"), "Source");
                            optionsBuilder.AddOption(ParseSectionWithTypePropertyNameToOptions(configuration, "Target", "$type"), "Target");
                        } else
                        {
                            optionsBuilder.AddOption(ParseSectionCollectionWithPathAsTypeToOption(configuration, "Endpoints:AzureDevOpsEndpoints", "Source"), "Source");
                            optionsBuilder.AddOption(ParseSectionCollectionWithPathAsTypeToOption(configuration, "Endpoints:AzureDevOpsEndpoints", "Target"), "Target");
                        }
                        break;
                    case MigrationConfigSchema.v160:
                        optionsBuilder.AddOption(ParseSectionWithTypePropertyNameToOptions(configuration, "MigrationTools:Endpoints:Source", "EndpointType"), "Source");
                        optionsBuilder.AddOption(ParseSectionWithTypePropertyNameToOptions(configuration, "MigrationTools:Endpoints:Target", "EndpointType"), "Target");
                        optionsBuilder.AddOption(ParseSectionListWithPathAsTypeToOption(configuration, "MigrationTools:CommonTools"));
                        optionsBuilder.AddOption(ParseSectionCollectionWithTypePropertyNameToList(configuration, "MigrationTools:CommonTools:FieldMappingTool:FieldMaps", "FieldMapType"));
                        optionsBuilder.AddOption(ParseSectionCollectionWithTypePropertyNameToList(configuration, "MigrationTools:Processors", "ProcessorType"));
                        break;
                }

                string json = optionsBuilder.Build();
                configFile = AddSuffixToFileName(configFile, "-upgraded");
                File.WriteAllText(configFile, json);
                _logger.LogInformation("New {configFile} file has been created", configFile);
                Console.WriteLine(json);

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

        private IOptions ParseSectionCollectionWithPathAsTypeToOption(IConfiguration configuration, string path, string filter)
        {
            var optionsConfigList = configuration.GetSection(path);
            var optionTypeString = GetLastSegment(path);
            IOptions option = null ;
            foreach (var childSection in optionsConfigList.GetChildren())
            {
                if (childSection.GetValue<string>("Name") == filter)
                {
                    option = GetOptionFromTypeString(configuration, childSection, optionTypeString);

                }                
            }
            return option;
        }

        private List<IOptions> ParseSectionListWithPathAsTypeToOption(IConfiguration configuration, string path)
        {
            var optionsConfigList = configuration.GetSection(path);
            List<IOptions> options = new List<IOptions>();
            foreach (var childSection in optionsConfigList.GetChildren())
            {
                var optionTypeString = childSection.Key;
                var option = GetOptionFromTypeString(configuration, childSection, optionTypeString);
                if (option != null)
                {
                    options.Add(option);
                }                
            }
            return options;
        }

        private List<IOptions> ParseSectionCollectionWithTypePropertyNameToList(IConfiguration configuration, string path, string typePropertyName)
        {
            var targetSection = configuration.GetSection(path);
            List<IOptions> options = new List<IOptions>();
            foreach (var childSection in targetSection.GetChildren())
            {
                var optionTypeString = childSection.GetValue<string>(typePropertyName);
                var newOptionTypeString = ParseOptionsType(optionTypeString);
                _logger.LogInformation("Upgrading {group} item {old} to {new}", path, optionTypeString, newOptionTypeString);
                var option = GetOptionWithDefaults(configuration, newOptionTypeString);
                childSection.Bind(option);
                options.Add(option);
            }

            return options;
        }

        private List<IOptions> ParseV1FieldMaps(IConfiguration configuration)
        {
            List<IOptions> options = new List<IOptions>();
            _logger.LogInformation("Upgrading {old} to {new}", "FieldMaps", "FieldMappingToolOptions");
            var toolOption = GetOptionWithDefaults(configuration, ParseOptionsType("FieldMappingToolOptions"));
            toolOption.Enabled = true;
            options.Add(toolOption);
            // parese FieldMaps
            options.AddRange(ParseSectionCollectionWithTypePropertyNameToList(configuration, "FieldMaps", "$type"));
            return options;
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

        private IOptions ParseSectionWithTypePropertyNameToOptions(IConfiguration configuration, string path, string typePropertyName)
        {
            var optionsConfig = configuration.GetSection(path);
            var optionTypeString = optionsConfig.GetValue<string>(typePropertyName);
            IOptions sourceOptions = GetOptionFromTypeString(configuration, optionsConfig, optionTypeString);
            return sourceOptions;
        }

        private IOptions GetOptionFromTypeString(IConfiguration configuration, IConfigurationSection optionsConfig, string optionTypeString)
        {
            var newOptionTypeString = ParseOptionsType(optionTypeString);
            _logger.LogInformation("Upgrading to {old} to {new}", optionTypeString, newOptionTypeString);
            IOptions sourceOptions;
            sourceOptions = GetOptionWithDefaults(configuration, newOptionTypeString);
            optionsConfig.Bind(sourceOptions);
            return sourceOptions;
        }

        private IOptions GetOptionWithDefaults(IConfiguration configuration, string optionTypeString)
        {
            IOptions option;
            optionTypeString = ParseOptionsType(optionTypeString);
            var optionType = AppDomain.CurrentDomain.GetMigrationToolsTypes().WithInterface<IOptions>().FirstOrDefault(t => t.Name.StartsWith(optionTypeString, StringComparison.InvariantCultureIgnoreCase));
            if (optionType == null)
            {
                _logger.LogWarning("Could not find type {optionTypeString}", optionTypeString);
                return null;
            }
            option = (IOptions)Activator.CreateInstance(optionType);
            var defaultConfig = configuration.GetSection(option.ConfigurationMetadata.PathToDefault);
            defaultConfig.Bind(option);
            return option;
        }

        private IOptions ParseV1TfsChangeSetMappingToolOptions(IConfiguration configuration)
        {
            _logger.LogInformation("Upgrading {old} to {new}", "ChangeSetMappingFile", "TfsChangeSetMappingTool");
            var changeSetMappingOptions = configuration.GetValue<string>("ChangeSetMappingFile");
            var properties = new Dictionary<string, object>
                        {
                            { "ChangeSetMappingFile", changeSetMappingOptions }
                        };
            var option = (IOptions)OptionsBinder.BindToOptions("TfsChangeSetMappingToolOptions", properties, classNameChangeLog);
            option.Enabled = true;
            return option;
        }

        private IOptions ParseV1TfsGitRepoMappingOptions(IConfiguration configuration)
        {
            _logger.LogInformation("Upgrading {old} to {new}", "GitRepoMapping", "TfsGitRepoMappingTool");
            var data = configuration.GetValue<Dictionary<string, string>>("GitRepoMapping");
            var properties = new Dictionary<string, object>
                        {
                            { "Mappings", data }
                        };
            var option = (IOptions)OptionsBinder.BindToOptions("TfsGitRepositoryToolOptions", properties, classNameChangeLog);
            option.Enabled = true;
            return option;
        }

        static string ParseOptionsType(string optionTypeString)
        {
            if (classNameChangeLog.ContainsKey(optionTypeString))
            {
                optionTypeString = classNameChangeLog[optionTypeString];
            }
            return RemoveSuffix(optionTypeString);
        }

        static string RemoveSuffix(string input)
        {
            // Use regex to replace "Config" or "Options" only if they appear at the end of the string
            return Regex.Replace(input, "(s|Config|Options)$", "");
        }

        static string GetLastSegment(string path)
        {
            // Split the path by colon and return the last segment
            string[] segments = path.Split(':');
            return segments[segments.Length - 1];
        }

        static bool IsSectionNullOrEmpty(IConfigurationSection section)
        {
            // Check if the section exists and has a value or children
            return !section.Exists() || string.IsNullOrEmpty(section.Value) && !section.GetChildren().Any();
        }


    }
}
