using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using Elmah.Io.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Services.Audit;
using Microsoft.VisualStudio.Services.Common.CommandLine;
using MigrationTools.EndpointEnrichers;
using MigrationTools.Endpoints.Infrastructure;
using MigrationTools.Enrichers;
using Newtonsoft.Json.Linq;
using Serilog.Core;

namespace MigrationTools.Options
{
    public  class OptionsConfigurationBuilder
    {
        readonly ILogger logger;
        readonly IConfiguration configuration;

        private List<IOptions> OptionsToInclude { get; }
        private Dictionary<string, IOptions> NamedOptionsToInclude { get; }

        private List<Type> catalogue;

        public OptionsConfigurationBuilder(
            IConfiguration configuration,
            ILogger<OptionsConfigurationBuilder> logger,
            ITelemetryLogger telemetryLogger)
        {
            this.configuration = configuration;
            this.logger = logger;
            OptionsToInclude = new List<IOptions>();
            NamedOptionsToInclude = new Dictionary<string, IOptions>();
            catalogue = AppDomain.CurrentDomain.GetMigrationToolsTypes().WithInterface<IOptions>().ToList();
        }

        public void AddAllOptions()
        {
            var keyGen = new KeyGenerator();

            foreach (var optionType in catalogue)
            {
                switch (optionType)
                {
                    case Type t when typeof(IEndpointOptions).IsAssignableFrom(t):
                        AddOption(optionType.Name, keyGen.GetNextKey());
                        break;
                    case Type t when typeof(IProcessorEnricherOptions).IsAssignableFrom(t):
                        logger.LogWarning("Skipping ProcessorEnricherOptions: {optionType}", optionType.Name);
                        break;
                    case Type t when typeof(IEndpointEnricherOptions).IsAssignableFrom(t):
                        logger.LogWarning("Skipping ProcessorEnricherOptions: {optionType}", optionType.Name);
                        break;
                    default:
                        AddOption(optionType.Name);
                        break;
                }
            }
        }

        public void AddOption(IOptions option)
        {
            if (option != null)
            {
                OptionsToInclude.Add(option);
            } else
            {
                logger.LogWarning("Could not add option as it was null");
            }
        }

        public void AddOption(IEnumerable<IOptions> options)
        {
            if (options != null)
            {
                OptionsToInclude.AddRange(options);
            } else
            {
                logger.LogWarning("Could not add options as they were null");
            }
        }

        public void AddOption(string optionName)
        {
            optionName = optionName.Replace("Options", "");
            var optionType = catalogue.FirstOrDefault(x => x.Name.StartsWith(optionName));
            if (optionType == null)
            {
                logger.LogWarning("Could not find option type for {optionName}", optionName);
            } else
            {
                logger.LogDebug("Adding {optionName}", optionName);
                OptionsToInclude.Add(CreateOptionFromType(optionType));
            }
            
        }

        private IOptions CreateOptionFromType(Type optionType)
        {
            IOptions instanceOfOption = (IOptions)Activator.CreateInstance(optionType);
            var section = configuration.GetSection(instanceOfOption.ConfigurationMetadata.PathToSample);
            section.Bind(instanceOfOption);
            return instanceOfOption;
        }

        public void AddOption(IOptions option, string key)
        {
            if (option != null)
            {
                NamedOptionsToInclude.Add(key, option);
            } else
            {
                logger.LogWarning("Could not add option as it was null");
            }
        }

        public void AddOption(string optionName, string key)
        {
            optionName = optionName.Replace("Options", "");
            var optionType = catalogue.FirstOrDefault(x => x.Name.StartsWith(optionName));
            if (optionType == null)
            {
                logger.LogWarning("Could not find option type for {optionName}", optionName);
            }
            else
            {
                logger.LogDebug("Adding {optionName} as {key}", optionName, key);
                NamedOptionsToInclude.Add(key, CreateOptionFromType(optionType));
            }            
        }

        public string Build()
        {
            logger.LogInformation("Building Configuration");
            JObject configJson = new JObject();
            configJson["Serilog"] = new JObject();
            configJson["Serilog"]["MinimumLevel"] = $"Information";
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            configJson["MigrationTools"] = new JObject();
            configJson["MigrationTools"]["Version"] = $"{version.Major}.{version.Minor}";
            configJson["MigrationTools"]["Endpoints"] = new JObject();
            configJson["MigrationTools"]["Processors"] = new JArray();
            configJson["MigrationTools"]["CommonTools"] = new JObject();
            foreach (var item in OptionsToInclude)
            {
                configJson = AddOptionToConfig(configuration, configJson, item);
            }
            foreach (var item in NamedOptionsToInclude)
            {
                configJson = AddNamedOptionToConfig(configuration, configJson, item.Key, item.Value);
            }
            return configJson.ToString(Newtonsoft.Json.Formatting.Indented);
        }

        private JObject AddNamedOptionToConfig(IConfiguration configuration, JObject configJson, string key, IOptions option)
        {
            if (option.ConfigurationMetadata.PathToInstance == null)
            {
                logger.LogWarning("Skipping Option: {item} with {key} as it has no PathToInstance", option.GetType().Name, key);
                return configJson;
            }
            try
            {
                var hardPath = $"MigrationTools:Endpoints:{key}";
                logger.LogDebug("Building Option: {item} to {hardPath}", option.GetType().Name, hardPath);
                configJson = OptionsConfigurationCompiler.AddOptionsToConfiguration(configJson, option, hardPath, true);
                
            }
            catch (Exception)
            {

                logger.LogWarning("FAILED!! Adding Option: {item}", option.GetType().FullName);
            }

            return configJson;
        }

        private JObject AddOptionToConfig(IConfiguration configuration, JObject configJson, IOptions option)
        {
            if (option is null)
            {
                logger.LogWarning("Skipping Option: as it is null");
                return configJson;
            }
            if (option.ConfigurationMetadata.PathToInstance == null)
            {
                logger.LogWarning("Skipping Option: {item} as it has no PathToInstance", option.GetType().Name);
                return configJson;
            }
            try
            {
                logger.LogDebug("Building Option: {item} to {path}", option.GetType().Name, option.ConfigurationMetadata.PathToInstance);
                configJson = OptionsConfigurationCompiler.AddOptionsToConfiguration(configJson, option, false);
                
            }
            catch (Exception)
            {

                logger.LogWarning("FAILED!! Adding Option: {item}", option.GetType().FullName);
            }

            return configJson;
        }

        
    }

    public class KeyGenerator
    {
        private int _counter = 1;

        public string GetNextKey()
        {
            _counter++;
            return $"Key{_counter}";
        }
    }
}
