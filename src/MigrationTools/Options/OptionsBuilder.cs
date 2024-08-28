using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using Elmah.Io.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Services.Audit;
using Microsoft.VisualStudio.Services.Common.CommandLine;
using Newtonsoft.Json.Linq;

namespace MigrationTools.Options
{
    public  class OptionsBuilder
    {
        readonly ILogger logger;
        readonly IConfiguration configuration;

        private List<IOptions> OptionsToInclude { get; }
        private Dictionary<string, IOptions> NamedOptionsToInclude { get; }

        private List<Type> catalogue;

        public OptionsBuilder(
            IConfigurationRoot configuration,
            ILogger<OptionsBuilder> logger,
            ITelemetryLogger telemetryLogger)
        {
            this.configuration = configuration;
            this.logger = logger;
            OptionsToInclude = new List<IOptions>();
            NamedOptionsToInclude = new Dictionary<string, IOptions>();
            catalogue = AppDomain.CurrentDomain.GetMigrationToolsTypes().WithInterface<IOptions>().ToList();
        }

        public void AddOption(IOptions option)
        {
            OptionsToInclude.Add(option);
        }

        public void AddOption(string optionName)
        {
            optionName = optionName.Replace("Options", "").Replace("Config", "");
            var optionType = catalogue.FirstOrDefault(x => x.Name.StartsWith(optionName));
            OptionsToInclude.Add(CreateOptionFromType(optionType));
        }

        private IOptions CreateOptionFromType(Type optionType)
        {
            IOptions instanceOfOption = (IOptions)Activator.CreateInstance(optionType);
            var section = configuration.GetSection(instanceOfOption.ConfigurationMetadata.PathToInstance);
            section.Bind(instanceOfOption);
            return instanceOfOption;
        }

        public void AddOption(IOptions option, string key)
        {
            NamedOptionsToInclude.Add(key, option);
        }

        public void AddOption(string optionName, string key)
        {
            optionName = optionName.Replace("Options", "").Replace("Config", "");
            var optionType = catalogue.FirstOrDefault(x => x.Name.StartsWith(optionName));
            NamedOptionsToInclude.Add(key, CreateOptionFromType(optionType));
        }

        public string Build()
        {
            JObject configJson = new JObject();
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
            try
            {
                var hardPath = $"MigrationTools:Endpoints:{key}";
                configJson = Options.OptionsManager.AddOptionsToConfiguration(configJson, option, hardPath, true);
                logger.LogInformation("Adding Option: {item}", option.GetType().Name);
            }
            catch (Exception)
            {

                logger.LogInformation("FAILED!! Adding Option: {item}", option.GetType().FullName);
            }

            return configJson;
        }

        private JObject AddOptionToConfig(IConfiguration configuration, JObject configJson, IOptions option)
        {
            try
            {
                configJson = Options.OptionsManager.AddOptionsToConfiguration(configJson, option, false);
                logger.LogInformation("Adding Option: {item}", option.GetType().Name);
            }
            catch (Exception)
            {

                logger.LogInformation("FAILED!! Adding Option: {item}", option.GetType().FullName);
            }

            return configJson;
        }
    }
}
