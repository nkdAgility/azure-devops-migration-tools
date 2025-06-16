using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
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
using MigrationTools.Services;
using Newtonsoft.Json.Linq;
using static System.Collections.Specialized.BitVector32;

namespace MigrationTools.Options
{
    public class OptionsConfigurationUpgrader
    {
        public IServiceProvider Services { get; }

        readonly ILogger _logger;
        private static Dictionary<string, string> classNameChangeLog = new Dictionary<string, string>();

        private List<Type> catalogue;

        public OptionsConfigurationUpgrader(
            IConfiguration configuration,
            ILogger<OptionsConfigurationBuilder> logger,
            ITelemetryLogger telemetryLogger, IServiceProvider services)
        {
            this.Services = services;
            this._logger = logger;
            catalogue = AppDomain.CurrentDomain.GetMigrationToolsTypes().WithInterface<IOptions>().ToList();
            configuration.GetSection("MigrationTools:Infrastructure:ClassNameChangeMappings").Bind(classNameChangeLog);
            if (classNameChangeLog.Count == 0)
            {
                _logger.LogWarning("No ClassNameChangeMappings found in configuration! Check the value of MigrationTools:Infrastructure:ClassNameChangeMappings in the appsettings.json file!");
            }
        }

        public OptionsConfigurationBuilder UpgradeConfiguration(OptionsConfigurationBuilder optionsBuilder, string configurationFile)
        {
            using (var activity = ActivitySourceProvider.ActivitySource.StartActivity("OptionsConfigurationUpgrader::UpgradeConfiguration"))
            {
                if (optionsBuilder == null)
                {
                    optionsBuilder = this.Services.GetService<OptionsConfigurationBuilder>();
                }

                if (string.IsNullOrEmpty(configurationFile))
                {
                    configurationFile = "configuration.json";
                }
                _logger.LogInformation("Importing: {configFile}", configurationFile);

                // Load configuration
                var configuration = new ConfigurationBuilder()
                    .AddJsonFile(configurationFile, optional: false, reloadOnChange: false)
                    .Build();


                var schemaVersion = VersionOptions.ConfigureOptions.GetMigrationConfigVersion(configuration);
                activity?.AddTag("SchemaVersion", schemaVersion.schema.ToString());
                activity.AddEvent(new ActivityEvent($"UpgradeConfigCommand.{schemaVersion.schema.ToString()}"));
                switch (schemaVersion.schema)
                {
                    case MigrationConfigSchema.v1:
                    case MigrationConfigSchema.v150:
                        activity.AddEvent(new ActivityEvent("UpgradeConfigCommand.v150"));
                        // ChangeSetMappingFile
                        optionsBuilder.AddOption(ParseV1TfsChangeSetMappingToolOptions(configuration));
                        optionsBuilder.AddOption(ParseV1TfsGitRepoMappingOptions(configuration));
                        optionsBuilder.AddOption(ParseV1FieldMaps(configuration));
                        optionsBuilder.AddOption(ParseSectionCollectionWithTypePropertyNameToList(configuration, "Processors", "$type"));
                        optionsBuilder.AddOption(ParseSectionCollectionWithTypePropertyNameToList(configuration, "CommonEnrichersConfig", "$type"));
                        optionsBuilder.AddOption(ParseSectionCollectionWithTypePropertyNameToList(configuration, "CommonTools", "$type"));
                        if (!IsSectionNullOrEmpty(configuration.GetSection("Source")) || !IsSectionNullOrEmpty(configuration.GetSection("Target")))
                        {
                            optionsBuilder.AddOption(ParseSectionWithTypePropertyNameToOptions(configuration, "Source", "$type"), "Source");
                            optionsBuilder.AddOption(ParseSectionWithTypePropertyNameToOptions(configuration, "Target", "$type"), "Target");
                        }
                        else
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
                return optionsBuilder;
            }
        }

        private IOptions ParseSectionCollectionWithPathAsTypeToOption(IConfiguration configuration, string path, string filter)
        {
            var optionsConfigList = configuration.GetSection(path);
            var optionTypeString = GetLastSegment(path);
            IOptions option = null;
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
                _logger.LogDebug("Upgrading {group} item {old} to {new}", path, optionTypeString, newOptionTypeString);
                var option = GetOptionWithDefaults(configuration, newOptionTypeString);
                if (option != null)
                {
                    childSection.Bind(option);
                    switch (optionTypeString)
                    {
                        case "TfsNodeStructureOptions":
                            MapTfsNodeStructureOptions(childSection, option);
                            break;
                        default:
                            break;
                    }
                    options.Add(option);
                }
                else
                {
                    _logger.LogWarning("Could not create option for type {newOptionTypeString} (original: {optionTypeString})", newOptionTypeString, optionTypeString);
                }
            }

            return options;
        }

        private void MapTfsNodeStructureOptions(IConfigurationSection section, dynamic option)
        {
            // Handle conversion from old dictionary format to new array format for Areas.Mappings
            var areasMappingsSection = section.GetSection("Areas:Mappings");
            if (areasMappingsSection.Exists() && areasMappingsSection.GetChildren().Any())
            {
                // Check if this is a dictionary format (children have Key/Value pairs rather than indexed)
                var firstChild = areasMappingsSection.GetChildren().FirstOrDefault();
                if (firstChild != null && !int.TryParse(firstChild.Key, out _))
                {
                    // This is dictionary format, convert to NodeMapping objects
                    option.Areas.Mappings.Clear();
                    foreach (var mapping in areasMappingsSection.GetChildren())
                    {
                        // Create NodeMapping using reflection or dynamic approach
                        dynamic nodeMapping = CreateNodeMapping(mapping.Key, mapping.Value);
                        option.Areas.Mappings.Add(nodeMapping);
                    }
                    _logger.LogDebug("Converted Areas.Mappings from dictionary format to array format for TfsNodeStructureTool");
                }
            }

            // Handle conversion from old dictionary format to new array format for Iterations.Mappings
            var iterationsMappingsSection = section.GetSection("Iterations:Mappings");
            if (iterationsMappingsSection.Exists() && iterationsMappingsSection.GetChildren().Any())
            {
                // Check if this is a dictionary format (children have Key/Value pairs rather than indexed)
                var firstChild = iterationsMappingsSection.GetChildren().FirstOrDefault();
                if (firstChild != null && !int.TryParse(firstChild.Key, out _))
                {
                    // This is dictionary format, convert to NodeMapping objects
                    option.Iterations.Mappings.Clear();
                    foreach (var mapping in iterationsMappingsSection.GetChildren())
                    {
                        // Create NodeMapping using reflection or dynamic approach
                        dynamic nodeMapping = CreateNodeMapping(mapping.Key, mapping.Value);
                        option.Iterations.Mappings.Add(nodeMapping);
                    }
                    _logger.LogDebug("Converted Iterations.Mappings from dictionary format to array format for TfsNodeStructureTool");
                }
            }

            // Legacy support: Map AreaMaps from the old structure to the new Areas.Mappings (if present)
            var areaMaps = section.GetSection("AreaMaps").GetChildren();
            foreach (var areaMap in areaMaps)
            {
                var key = areaMap.Key;
                var value = areaMap.Value;
                dynamic nodeMapping = CreateNodeMapping(key, value);
                option.Areas.Mappings.Add(nodeMapping);
            }

            // Legacy support: Map IterationMaps from the old structure to the new Iterations.Mappings (if present)
            var iterationMaps = section.GetSection("IterationMaps").GetChildren();
            foreach (var iterationMap in iterationMaps)
            {
                var key = iterationMap.Key;
                var value = iterationMap.Value;
                dynamic nodeMapping = CreateNodeMapping(key, value);
                option.Iterations.Mappings.Add(nodeMapping);
            }

            _logger.LogDebug("Mapped TfsNodeStructureOptions to TfsNodeStructureTool structure and updated the options object.");
        }

        private object CreateNodeMapping(string match, string replacement)
        {
            // Try to find the NodeMapping type from loaded assemblies
            var nodeMappingType = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .FirstOrDefault(t => t.Name == "NodeMapping" && t.Namespace == "MigrationTools.Tools");

            if (nodeMappingType != null)
            {
                var nodeMapping = Activator.CreateInstance(nodeMappingType);
                var matchProperty = nodeMappingType.GetProperty("Match");
                var replacementProperty = nodeMappingType.GetProperty("Replacement");

                if (matchProperty != null && replacementProperty != null)
                {
                    matchProperty.SetValue(nodeMapping, match);
                    replacementProperty.SetValue(nodeMapping, replacement);
                }

                return nodeMapping;
            }

            // Fallback: create a simple object with the properties
            _logger.LogWarning("Could not find NodeMapping type, creating fallback object");
            return new { Match = match, Replacement = replacement };
        }


        private List<IOptions> ParseV1FieldMaps(IConfiguration configuration)
        {
            List<IOptions> options = new List<IOptions>();
            _logger.LogDebug("Upgrading {old} to {new}", "FieldMaps", "FieldMappingToolOptions");
            var toolOption = GetOptionWithDefaults(configuration, ParseOptionsType("FieldMappingToolOptions"));
            toolOption.Enabled = true;
            options.Add(toolOption);
            // parese FieldMaps
            options.AddRange(ParseSectionCollectionWithTypePropertyNameToList(configuration, "FieldMaps", "$type"));
            return options;
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
            _logger.LogDebug("Upgrading to {old} to {new}", optionTypeString, newOptionTypeString);
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
            
            // Skip if no ChangeSetMappingFile is configured
            if (string.IsNullOrEmpty(changeSetMappingOptions))
            {
                _logger.LogDebug("No ChangeSetMappingFile found, skipping TfsChangeSetMappingTool creation");
                return null;
            }
            
            var properties = new Dictionary<string, object>
                        {
                            { "ChangeSetMappingFile", changeSetMappingOptions }
                        };
            var option = (IOptions)OptionsBinder.BindToOptions("TfsChangeSetMappingToolOptions", properties, classNameChangeLog);
            if (option != null)
            {
                option.Enabled = true;
            }
            return option;
        }

        private IOptions ParseV1TfsGitRepoMappingOptions(IConfiguration configuration)
        {
            _logger.LogInformation("Upgrading {old} to {new}", "GitRepoMapping", "TfsGitRepoMappingTool");
            var data = configuration.GetValue<Dictionary<string, string>>("GitRepoMapping");
            
            // Skip if no GitRepoMapping is configured
            if (data == null || data.Count == 0)
            {
                _logger.LogDebug("No GitRepoMapping found, skipping TfsGitRepoMappingTool creation");
                return null;
            }
            
            var properties = new Dictionary<string, object>
                        {
                            { "Mappings", data }
                        };
            var option = (IOptions)OptionsBinder.BindToOptions("TfsGitRepositoryToolOptions", properties, classNameChangeLog);
            if (option != null)
            {
                option.Enabled = true;
            }
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
