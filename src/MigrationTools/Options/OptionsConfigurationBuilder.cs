using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using Elmah.Io.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.Framework.Common;
using Microsoft.VisualStudio.Services.Audit;
using Microsoft.VisualStudio.Services.Common.CommandLine;
using MigrationTools.EndpointEnrichers;
using MigrationTools.Endpoints.Infrastructure;
using MigrationTools.Enrichers;
using MigrationTools.Processors.Infrastructure;
using MigrationTools.Tools.Infrastructure;
using Newtonsoft.Json.Linq;
using Serilog.Core;

namespace MigrationTools.Options
{

  

    public  class OptionsConfigurationBuilder
    {
        

        readonly ILogger logger;
        readonly IConfiguration configuration;
        

        OptionsContainer optionsContainer;

        private List<Type> catalogue;

        public OptionsConfigurationBuilder(
            IConfiguration configuration,
            ILogger<OptionsConfigurationBuilder> logger,
            ITelemetryLogger telemetryLogger)
        {
            this.configuration = configuration;
            this.logger = logger;
            optionsContainer = new OptionsContainer();
            catalogue = AppDomain.CurrentDomain.GetMigrationToolsTypes().WithInterface<IOptions>().ToList();
        }

        public IReadOnlyList<OptionItem> GetOptions()
        {
            return optionsContainer.GetOptions();
        }

        public IReadOnlyList<OptionItem> GetOptions(OptionItemType optionType)
        {
            return optionsContainer.GetOptions(optionType);
        }

        public void AddAllOptions()
        {
            foreach (var optionType in catalogue)
            {
                optionsContainer.AddOption(new OptionItem(CreateOptionFromType(optionType)));
            }
        }

        public void AddOption(IOptions option)
        {
            if (option != null)
            {
                optionsContainer.AddOption(new OptionItem(option));
            }
            else
            {
                logger.LogWarning("Could not add Option as it was null!");
            }
        }


        public void AddOption(OptionItem option)
        {
            if (option != null && option.Option !=null )
            {
                optionsContainer.AddOption(option);
            } else
            {
                logger.LogWarning("Could not add OptionItem as it was null or its IOption was!");
            }
        }

        public void AddOption(IEnumerable<IOptions> options)
        {
            if (options != null)
            {
                foreach (var item in options)
                {
                    AddOption(item);
                }
            } else
            {
                logger.LogWarning("Could not add options as they were null");
            }
        }

        public void AddOption(string optionName)
        {
            AddOption(CreateOptionFromString(optionName));            
        }
        private IOptions CreateOptionFromString(string optionName)
        {
            optionName = optionName.Replace("Options", "");
            var optionType = catalogue.FirstOrDefault(x => x.Name.StartsWith(optionName));
            return CreateOptionFromType(optionType);
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
                optionsContainer.AddOption(new OptionItem(key, option));
            } else
            {
                logger.LogWarning("Could not add option as it was null");
            }
        }

        public void AddOption(string optionName, string key)
        {
            optionsContainer.AddOption(new OptionItem(key, CreateOptionFromString(optionName)));     
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
            foreach (var item in optionsContainer.GetOptions())
            {
                if (item.IsKeyRequired)
                {
                    configJson = AddNamedOptionToConfig(configuration, configJson, item.key, item.Option);
                }
                else
                {
                    configJson = AddOptionToConfig(configuration, configJson, item.Option);
                }
               
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
        private Dictionary<string, int> _nameCounters = new Dictionary<string, int>();

        // Generate a key without a name (global counter)
        public string GetNextKey()
        {
            _counter++;
            return $"Refname{_counter}";
        }

        // Generate a key for a specific name (name-based counter)
        public string GetNextKey(string name)
        {
            // Check if the name already exists in the dictionary
            if (!_nameCounters.ContainsKey(name))
            {
                // If not, initialize its counter to 1
                _nameCounters[name] = 1;
            }
            else
            {
                // Increment the counter for the given name
                _nameCounters[name]++;
            }

            return $"{name}Refname{_nameCounters[name]}";
        }
    }


    public enum OptionItemType
    {
        Unknown,
        Processor,
        EndpointEnricher,
        ProcessorEnricher,
        Endpoint,
        Tool,
    }

    public class OptionItem
    {
        private static readonly KeyGenerator keyGen = new KeyGenerator();
        public OptionItemType Type { get; set; }
        public bool IsKeyRequired { get; }
        public string key { get; set; }
        public IOptions Option { get; set; }

        public OptionItem(string key, IOptions option)
        {
            Type = GetOptionItemType(option);
            IsKeyRequired = GetOptionItemKeyRequired(option);
            this.key = key;
            Option = option;
        }

        public OptionItem(IOptions option)
        {
            Type = GetOptionItemType(option);
            IsKeyRequired = GetOptionItemKeyRequired(option);
            this.key = keyGen.GetNextKey(Type.ToString());
            Option = option;
        }

        private static OptionItemType GetOptionItemType(IOptions option)
        {
            OptionItemType optionItemType;
            var optionType = option.GetType();
            switch (optionType)
            {
                case Type t when typeof(IEndpointOptions).IsAssignableFrom(t):
                    optionItemType = OptionItemType.Endpoint;
                    break;
                case Type t when typeof(IProcessorEnricherOptions).IsAssignableFrom(t):
                    optionItemType = OptionItemType.ProcessorEnricher;
                    break;
                case Type t when typeof(IEndpointEnricherOptions).IsAssignableFrom(t):
                    optionItemType = OptionItemType.EndpointEnricher;
                    break;
                case Type t when typeof(IProcessorOptions).IsAssignableFrom(t):
                    optionItemType = OptionItemType.Processor;
                    break;
                case Type t when typeof(IToolOptions).IsAssignableFrom(t):
                    optionItemType = OptionItemType.Tool;
                    break;
                default:
                    optionItemType = OptionItemType.Unknown;
                    break;
            }

            return optionItemType;
        }
        private static bool GetOptionItemKeyRequired(IOptions option)
        {
            var optionType = option.GetType();
            switch (optionType)
            {
                case Type t when typeof(IEndpointOptions).IsAssignableFrom(t):
                    return true;
                default:
                    return false;
            }
        }


    }

    internal class OptionsContainer
    {
        private List<OptionItem> OptionsToInclude { get; }

        public OptionsContainer()
        {
            OptionsToInclude = new List<OptionItem>();
        }

        public void AddOption(OptionItem option)
        {
            if (option != null)
            {
                OptionsToInclude.Add(option);
            }
        }

        public IReadOnlyList<OptionItem> GetOptions()
        {
            return OptionsToInclude
                .Select(option => option)
                .ToList()
                .AsReadOnly();
        }

        public IReadOnlyList<OptionItem> GetOptions(OptionItemType optionType)
        {
            return OptionsToInclude
                .Where(option => option.Type == optionType)
                .Select(option => option)
                .ToList()
                .AsReadOnly();
        }


    }

}
