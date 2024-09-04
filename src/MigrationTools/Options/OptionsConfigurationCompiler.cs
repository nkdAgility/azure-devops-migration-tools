using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;

namespace MigrationTools.Options
{


public static class OptionsConfigurationCompiler
    {
    public static IOptions LoadConfiguration(string filePath, IOptions options, bool isCollection = false)
    {
        var optionsConfig = options.ConfigurationMetadata;
        JObject json = File.Exists(filePath) ? JObject.Parse(File.ReadAllText(filePath)) : new JObject();

        // Determine the path based on whether this is a collection or a section
        string path = optionsConfig.PathToInstance;

        if (isCollection)
        {
            // Load from a collection
            var collection = json.SelectToken(path.Replace(":", ".")) as JArray;

            var item = collection?.FirstOrDefault(p => p[optionsConfig.ObjectName]?.ToString() == optionsConfig.OptionFor);

            return item != null ? item.ToObject(options.GetType()) as IOptions : Activator.CreateInstance(options.GetType()) as IOptions;
        }
        else
        {
            // Load from a section
            var section = json.SelectToken(path.Replace(":", "."));

            return section != null ? section.ToObject(options.GetType()) as IOptions : Activator.CreateInstance(options.GetType()) as IOptions;
        }
    }

    public static void SaveConfiguration(string filePath, IOptions options, bool isCollection = false)
    {
        JObject json = File.Exists(filePath) ? JObject.Parse(File.ReadAllText(filePath)) : new JObject();

        // Determine the path based on whether this is a collection or a section
        string path = options.ConfigurationMetadata.PathToInstance;

        string[] pathParts = path.Split(':');
        JObject currentSection = json;

        // Build the JSON structure for the section or collection
        for (int i = 0; i < pathParts.Length; i++)
        {
            if (i == pathParts.Length - 1 && isCollection)
            {
                // If it's a collection, create or find the JArray
                if (currentSection[pathParts[i]] == null)
                {
                    currentSection[pathParts[i]] = new JArray();
                }

                var collectionArray = (JArray)currentSection[pathParts[i]];

                // Check if the object already exists in the collection
                var existingItem = collectionArray.FirstOrDefault(p => p[options.ConfigurationMetadata.ObjectName]?.ToString() == options.ConfigurationMetadata.OptionFor);

                if (existingItem != null)
                {
                    // Update the existing item
                    var index = collectionArray.IndexOf(existingItem);
                    collectionArray[index] = JObject.FromObject(options);
                }
                else
                {
                    // Add the new item to the collection
                    var newItem = JObject.FromObject(options);
                    newItem[options.ConfigurationMetadata.ObjectName] = options.ConfigurationMetadata.OptionFor;
                    collectionArray.Add(newItem);
                }
            }
            else
            {
                // Create or navigate to the JObject for the section
                if (currentSection[pathParts[i]] == null)
                {
                    currentSection[pathParts[i]] = new JObject();
                }
                currentSection = (JObject)currentSection[pathParts[i]];
            }
        }

        // If it's not a collection, replace the content directly in the final section
        if (!isCollection)
        {
            currentSection.Replace(JObject.FromObject(options));
        }

        // Save the updated JSON file
        File.WriteAllText(filePath, json.ToString(Newtonsoft.Json.Formatting.Indented));
    }

    public static List<IOptions> LoadAll(string filePath, IOptions templateOption)
    {
        var optionsConfig = templateOption.ConfigurationMetadata;
        JObject json = File.Exists(filePath) ? JObject.Parse(File.ReadAllText(filePath)) : new JObject();

        var foundOptions = new List<IOptions>();

        // Recursively search through the entire JSON hierarchy
        SearchForOptions(json, optionsConfig, foundOptions, templateOption.GetType());

        return foundOptions;
    }

    private static void SearchForOptions(JToken token, ConfigurationMetadata config, List<IOptions> foundTools, Type optionType)
    {
        if (token is JObject obj)
        {
            // Check if this object has the appropriate property with the value matching the config
            if (obj.TryGetValue(config.ObjectName, out JToken fieldTypeToken) && fieldTypeToken.ToString() == config.OptionFor)
            {
                // Deserialize the JObject into an IOptions object
                var options = obj.ToObject(optionType) as IOptions;
                foundTools.Add(options);
            }

            // Recursively search child objects
            foreach (var property in obj.Properties())
            {
                SearchForOptions(property.Value, config, foundTools, optionType);
            }
        }
        else if (token is JArray array)
        {
            // Recursively search elements in the array
            foreach (var item in array)
            {
                SearchForOptions(item, config, foundTools, optionType);
            }
        }
    }

    public static string CreateNewConfigurationJson(IOptions options, string path, string objectName, string optionFor, bool isCollection = false, bool shouldAddObjectName = false)
    {
        // Load existing configuration from a file or create a new JObject if necessary
        JObject configJson = new JObject();

        // Add or update the options in the configuration using the new method signature
        configJson = AddOptionsToConfiguration(configJson, options, path, objectName, optionFor, isCollection, shouldAddObjectName);

        // Return the updated JSON as a formatted string
        return configJson.ToString(Newtonsoft.Json.Formatting.Indented);
    }


        public static JObject AddOptionsToConfiguration(JObject configJson, IOptions iOption, bool shouldAddObjectName = false)
        {
            return AddOptionsToConfiguration(configJson, iOption, iOption.ConfigurationMetadata.PathToInstance, shouldAddObjectName);
        }

        public static JObject AddOptionsToConfiguration(JObject configJson, IOptions iOption, string sectionPath, bool shouldAddObjectName = false)
        {
            return AddOptionsToConfiguration(configJson, iOption, sectionPath, iOption.ConfigurationMetadata.ObjectName, iOption.ConfigurationMetadata.OptionFor, iOption.ConfigurationMetadata.IsCollection, shouldAddObjectName);
        }

        public static JObject AddOptionsToConfiguration(
        JObject configJson,
        IOptions options,
        string path,
        string objectName,
        string optionFor,
        bool isCollection = false,
        bool shouldAddObjectName = false)
    {
        // Initialize the JObject if it was null
        if (configJson == null)
        {
            configJson = new JObject();
        }

        // Split the path into its components
        string[] pathParts = path.Split(':');
        JObject currentSection = configJson;

        // Traverse or create the JSON structure for the section or collection
        for (int i = 0; i < pathParts.Length; i++)
        {
            // If this is the last part of the path
            if (i == pathParts.Length - 1)
            {
                if (isCollection)
                {
                    // Ensure we have a JArray at this position
                    if (currentSection[pathParts[i]] == null)
                    {
                        currentSection[pathParts[i]] = new JArray();
                    }

                    // Add the options object as part of the collection
                    var collectionArray = (JArray)currentSection[pathParts[i]];
                    var optionsObject = JObject.FromObject(options);
                    // Always add object name for collections
                    optionsObject.AddFirst(new JProperty(objectName, optionFor));
                    collectionArray.Add(optionsObject);
                }
                else
                {
                    // We're at the last part of the path, so add the options object here
                    var optionsObject = new JObject();

                    // Add the object name and options
                    if (shouldAddObjectName)
                    {
                        optionsObject[objectName] = optionFor;
                    }

                    // Add the other properties from the options object
                    optionsObject.Merge(JObject.FromObject(options), new JsonMergeSettings
                    {
                        MergeArrayHandling = MergeArrayHandling.Concat
                    });

                    // Replace or add the object in the current section
                    currentSection[pathParts[i]] = optionsObject;
                }
            }
            else
            {
                // Traverse or create the JObject for the current section
                if (currentSection[pathParts[i]] == null)
                {
                    currentSection[pathParts[i]] = new JObject();
                }
                currentSection = (JObject)currentSection[pathParts[i]];
            }
        }

        // Return the modified JObject
        return configJson;
    }
}
}