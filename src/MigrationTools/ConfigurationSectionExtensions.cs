using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MigrationTools._EngineV1.Configuration;
using MigrationTools.Enrichers;
using MigrationTools.Options;
using MigrationTools.Tools.Infrastructure;
using Serilog;
using static System.Collections.Specialized.BitVector32;
using System.ComponentModel.DataAnnotations;

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
                Log.Warning("There was no match for {optionTypeName} as {processorTypeString} from {sectionKey} in {TMigrationOptions}", optionTypeName, processorTypeString, section.Key, typeof(TMigrationOptions));
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

        public static TOptions GetSectionCommonEnrichers_v15<TOptions>(this IConfiguration configuration) where TOptions : IOptions, new()
        {
            TOptions options = Activator.CreateInstance<TOptions>();
            var options_default = configuration.GetSection(options.ConfigurationMetadata.PathToInstance);
            var optionsclass = typeof(TOptions).Name;
            var options_v15 = configuration.GetSection("CommonEnrichersConfig").GetChildren().Where(x => x.GetValue<string>("$type") == optionsclass).FirstOrDefault();

            if (options_default.Exists())
            {
                options_default.Bind(options);
            }

            // Bind the second section, overriding or merging the values
            if (options_v15 != null && options_v15.Exists())
            {
                options_v15.Bind(options);
            }

            return options;
        }

    }
}
