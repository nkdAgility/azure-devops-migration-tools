using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace MigrationTools
{
    public static partial  class ConfigurationSectionExtensions
    {
        public static TMigrationOptions GetMigrationOptionFromConfig<TMigrationOptions>(this IConfigurationSection section)
        {
            // Get all loaded assemblies in the current application domain
            var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(ass => (ass.FullName.StartsWith("MigrationTools") || ass.FullName.StartsWith("VstsSyncMigrator")));
            // Get all types from each assembly
            IEnumerable<Type> typesWithConfigurationSectionName = assemblies
                         .SelectMany(assembly => assembly.GetTypes())
                         .Where(type => type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy).Any(field => field.IsLiteral && !field.IsInitOnly && field.Name == "ConfigurationSectionName"));
            var type = typesWithConfigurationSectionName.SingleOrDefault(type => type.GetField("ConfigurationSectionName").GetRawConstantValue().ToString() == section.Path);
            if (type != null)
            {
                TMigrationOptions options2 = (TMigrationOptions)section.Get(type);
                return options2;
            }
            Log.Debug($"MAINTAINER FAULT: Failed to find options for Section {section.Path}!");
            return default(TMigrationOptions);
            
        }
    }
}
