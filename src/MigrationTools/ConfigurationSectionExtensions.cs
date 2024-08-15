using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MigrationTools._EngineV1.Configuration;
using MigrationTools.Enrichers;
using Serilog;

namespace MigrationTools
{
    public static partial class ConfigurationSectionExtensions
    {
        public static List<TMigrationOptions> ToMigrationToolsList<TMigrationOptions>(this IConfigurationSection section, Func<IConfigurationSection, TMigrationOptions> childAction)
        {
            Log.Debug("===================================");
            Log.Debug("Configuring '{sectionPath}'", section.Path);
            List< TMigrationOptions > options = new List<TMigrationOptions>();
            bool anyFailures = false;
            foreach (var child in section.GetChildren())
            {
                Log.Debug("Configuring '{childKey}' as '{Name}' from '{sectionPath}'", child.Key, typeof(TMigrationOptions).Name, section.Path);
                TMigrationOptions option = childAction.Invoke(child);
                if (option != null)
                {
                    options.Add(option);
                } else
                {
                    anyFailures = true;
                }
            }
            if (anyFailures)
            {
                Log.Warning("-------------------------");
                Log.Warning("One or more {sectionPath} configuration items failed to load.", section.Path);
               Log.Warning("Available Options: @{typesWithConfigurationSectionName}", AppDomain.CurrentDomain.GetMigrationToolsTypes().WithInterface<TMigrationOptions>().Select(type => type.Name.Replace("Options", "").Replace("Config", "")));
                Log.Warning("These are the only valid option, so please check all of the items in the configuration file under {Parent}.", section.Path);
            }
            Log.Debug("===================================");
            return options;
        }

        public static TMigrationOptions GetMigrationToolsNamedOption<TMigrationOptions>(this IConfigurationSection section)
        {
            // Get all types from each assembly
            IEnumerable<Type> typesWithConfigurationSectionName = AppDomain.CurrentDomain.GetMigrationToolsTypes().WithInterface<TMigrationOptions>().WithConfigurationSectionName();

            var type = typesWithConfigurationSectionName.SingleOrDefault(type => type.GetField("ConfigurationSectionName").GetRawConstantValue().ToString() == section.Path);
            if (type == null)
            {
                Log.Warning("There was no match for {sectionKey}", section.Key);
                return default(TMigrationOptions);
            }
            TMigrationOptions options2 = (TMigrationOptions)section.Get(type);
            return options2;

        }

        public static TMigrationOptions GetMigrationToolsOption<TMigrationOptions>(this IConfigurationSection section, string optionTypeName)
        {
            var processorTypeString = section.GetValue<string>(optionTypeName);
            if (processorTypeString == null)
            {
                Log.Warning("There was no value for {optionTypeName} from {sectionKey}", optionTypeName, section.Key);
                return default(TMigrationOptions);
            }
            var processorType = AppDomain.CurrentDomain.GetMigrationToolsTypes().WithInterface<TMigrationOptions>().WithNameString(processorTypeString);
            if (processorType == null)
            {
                Log.Warning("There was no match for {optionTypeName} from {sectionKey}", optionTypeName, section.Key);
                return default(TMigrationOptions);
            }
            var obj = Activator.CreateInstance(processorType);
            section.Bind(obj);
            return (TMigrationOptions)obj;

        }

        public static IProcessorConfig GetMigrationToolsProcessorOption(this IConfigurationSection section)
        {
            return section.GetMigrationToolsOption<IProcessorConfig>("ProcessorType");
        }


    }

    public static partial class ConfigurationExtensions
    {
        public enum MigrationConfigVersion
        {
            v15,
            v16
        }

        public static IConfiguration GetSectionCommonEnrichers_v15<TEnricherOptions>(this IConfiguration configuration, string defaults) where TEnricherOptions : IProcessorEnricherOptions
        {
            var configOpptions = configuration.GetSection(defaults);
            var optionsclass = typeof(TEnricherOptions).Name;
            configOpptions.Bind(configuration.GetSection("CommonEnrichersConfig").GetChildren().Where(x => x.GetValue<string>("$type") == optionsclass).FirstOrDefault());
            return configOpptions;
        }

        public static MigrationConfigVersion GetMigrationConfigVersion(this IConfiguration configuration)
        {
            bool isOldFormat = false;
            string configVersionString = configuration.GetValue<string>("MigrationTools:Version");
            if (string.IsNullOrEmpty(configVersionString))
            {
                isOldFormat = true;
                configVersionString = configuration.GetValue<string>("Version");
            }
            if (string.IsNullOrEmpty(configVersionString))
            {
                configVersionString = "0.0";
            }
            Version.TryParse(configVersionString, out Version configVersion);
            if (configVersion < Version.Parse("16.0") || isOldFormat)
            {
                return MigrationConfigVersion.v15;
            }
            else
            {
                return MigrationConfigVersion.v16;
            }
        }

    }
}
