using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Options;
using MigrationTools;
using MigrationTools.Options;
using Serilog;

public static class OptionsBinder
{
    public static object BindToOptions(string typeName, Dictionary<string, object> properties, Dictionary<string, string> nameMappings)
    {
        // Get the type from the current AppDomain
        // Check if the property name needs to be mapped to a new name
        if (nameMappings.ContainsKey(typeName))
        {
            typeName = nameMappings[typeName];
        }
        var type = AppDomain.CurrentDomain.GetMigrationToolsTypes().WithInterface<IOptions>().FirstOrDefault(t => t.Name == typeName);

        if (type == null)
        {
            Log.Warning($"Type '{typeName}' not found.");
            return null;
        }

        // Create an instance of the type
        var optionsObject = Activator.CreateInstance(type);

        // Iterate over the dictionary and set the properties
        foreach (var property in properties)
        {
            var originalPropertyName = property.Key;
            var propertyValue = property.Value;

            // Get the PropertyInfo object representing the property
            var propertyInfo = type.GetProperty(originalPropertyName, BindingFlags.Public | BindingFlags.Instance);

            if (propertyInfo != null && propertyInfo.CanWrite)
            {
                // Convert the value to the correct type if necessary
                var convertedValue = Convert.ChangeType(propertyValue, propertyInfo.PropertyType);
                propertyInfo.SetValue(optionsObject, convertedValue);
            }
            else
            {
                // Log the missing property instead of throwing an exception
                Log.Warning($"Property '{originalPropertyName}' not found or is not writable on type '{typeName}'.");
            }
        }

        return optionsObject;
    }
}
