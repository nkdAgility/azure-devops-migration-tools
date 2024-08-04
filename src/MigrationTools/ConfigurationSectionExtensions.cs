using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MigrationTools._EngineV1.Configuration;
using Serilog;

namespace MigrationTools
{
    public static partial  class ConfigurationSectionExtensions
    {
        public static List<TMigrationOptions> ToMigrationToolsList<TMigrationOptions>(this IConfigurationSection section)
        {
            return section.GetChildren()?.ToList().ConvertAll<TMigrationOptions>(x => x.GetMigrationToolsNamedOption<TMigrationOptions>());
        }



        public static TMigrationOptions GetMigrationToolsNamedOption<TMigrationOptions>(this IConfigurationSection section)
        {
            // Get all types from each assembly
            IEnumerable<Type> typesWithConfigurationSectionName = AppDomain.CurrentDomain.GetMigrationToolsTypes().WithInterface<TMigrationOptions>().WithConfigurationSectionName();

            var type = typesWithConfigurationSectionName.SingleOrDefault(type => type.GetField("ConfigurationSectionName").GetRawConstantValue().ToString() == section.Path);
            if (type == null)
            {
                Log.Fatal("While processing `{path}` Could not find a class for {sectionKey}[Options|Config] that has a ConfigurationSectionName property that matches the path.", section.Path, section.Key);
                Log.Information("Please check the spelling of {key} in the config.", section.Key);
                Log.Information("Available Options: @{typesWithConfigurationSectionName}", typesWithConfigurationSectionName.Select(type => type.Name.Replace("Options", "").Replace("Config", "")));
                Log.Information("These are the only valid option, so please check all of the items in the configuration file under {Parent}.", section.Path.Substring(0, section.Path.LastIndexOf(":")));
                Environment.Exit(-1);
            }
            TMigrationOptions options2 = (TMigrationOptions)section.Get(type);
            return options2;

        }

        public static TMigrationOptions GetMigrationToolsOption<TMigrationOptions>(this IConfigurationSection section, string optionTypeName)
        {
            var processorTypeString = section.GetValue<string>(optionTypeName);
            if (processorTypeString == null)
            {
                throw new ConfigException($"There was no value for {optionTypeName} on {section.Key}");
                return default(TMigrationOptions);
            }
            var processorType = AppDomain.CurrentDomain.GetMigrationToolsTypes().WithNameString(processorTypeString);
            var obj = Activator.CreateInstance(processorType);
            section.Bind(obj);
            return (TMigrationOptions)obj;

        }

        public static IProcessorConfig GetMigrationToolsProcessorOption(this IConfigurationSection section)
        {
            return section.GetMigrationToolsOption<IProcessorConfig>("ProcessorType");
        }


    }
}
