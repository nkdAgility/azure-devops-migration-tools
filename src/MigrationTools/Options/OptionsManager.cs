using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.AccessControl;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MigrationTools.Options
{
    public class OptionsManager
    {

        public static dynamic GetOptionsManager(Type option)
        {
            Type optionsManagerType = typeof(OptionsManager<>);
            Type specificOptionsManagerType = optionsManagerType.MakeGenericType(option);

            object optionsManagerInstance = Activator.CreateInstance(
                specificOptionsManagerType
            );
            return optionsManagerInstance;
        }

        public static string CreateNewConfigurationJson(IOptions iOption, bool isCollection = false)
        {
            Type optionsManagerType = typeof(OptionsManager<>).MakeGenericType(iOption.GetType());

            // Create an instance of OptionsManager<T>
            object optionsManagerInstance = Activator.CreateInstance(optionsManagerType);

            // Get the method information for CreateNewConfigurationJson
            MethodInfo createMethod = optionsManagerType.GetMethod("CreateNewConfigurationJson");

            // Prepare parameters for the method
            object[] parameters = { iOption, isCollection };

            // Invoke the method dynamically
            string result = (string)createMethod.Invoke(optionsManagerInstance, parameters);

            // Output the result
            return result;
        }

        public static JObject AddOptionsToConfiguration(JObject configJson, IOptions iOption, bool shouldAddObjectName = false)
        {
            //JObject configJson, TOptions options, string path, string objectName, string optionFor, bool isCollection = false, bool shouldAddObjectName = false
            return AddOptionsToConfiguration(configJson, iOption, iOption.ConfigurationMetadata.PathToInstance,shouldAddObjectName);
        }

        public static JObject AddOptionsToConfiguration(JObject configJson, IOptions iOption, string sectionPath, bool shouldAddObjectName = false)
        {
            Type optionsManagerType = typeof(OptionsManager<>).MakeGenericType(iOption.GetType());

            // Create an instance of OptionsManager<T>
            object optionsManagerInstance = Activator.CreateInstance(optionsManagerType);

            // Get the method information for CreateNewConfigurationJson
            MethodInfo createMethod = optionsManagerType.GetMethod("AddOptionsToConfiguration");

            // Prepare parameters for the method
            object[] parameters = { configJson, iOption, sectionPath, iOption.ConfigurationMetadata.ObjectName, iOption.ConfigurationMetadata.OptionFor, iOption.ConfigurationMetadata.IsCollection, shouldAddObjectName  };

            // Invoke the method dynamically
            JObject result = (JObject)createMethod.Invoke(optionsManagerInstance, parameters);

            // Output the result
            return result;
        }

        public static ConfigurationMetadata GetOptionsConfiguration(Type option)
        {
            // ActivatorUtilities.CreateInstance(option);
            IOptions optionInsance = (IOptions)Activator.CreateInstance(option);
            return optionInsance.ConfigurationMetadata;
        }

    }

    public class OptionsManager<TOptions> where TOptions : class, IOptions, new()
    {
        public TOptions LoadConfiguration(string filePath, bool isCollection = false)
        {
            var optionsConfig = GetOptionsConfiguration();
            JObject json = File.Exists(filePath) ? JObject.Parse(File.ReadAllText(filePath)) : new JObject();

            // Determine the path based on whether this is a collection or a section
            string path = optionsConfig.PathToInstance;

            if (isCollection)
            {
                // Load from a collection
                var collection = json.SelectToken(path.Replace(":", ".")) as JArray;

                var item = collection?.FirstOrDefault(p => p[optionsConfig.ObjectName]?.ToString() == optionsConfig.OptionFor);

                return item != null ? item.ToObject<TOptions>() : new TOptions();
            }
            else
            {
                // Load from a section
                var section = json.SelectToken(path.Replace(":", "."));

                return section != null ? section.ToObject<TOptions>() : new TOptions();
            }
        }

        public void SaveConfiguration(string filePath, TOptions options, bool isCollection = false)
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
            File.WriteAllText(filePath, json.ToString(Formatting.Indented));
        }

        public List<TOptions> LoadAll(string filePath)
        {
            var optionsConfig = GetOptionsConfiguration();
            JObject json = File.Exists(filePath) ? JObject.Parse(File.ReadAllText(filePath)) : new JObject();

            var foundOptions = new List<TOptions>();

            // Recursively search through the entire JSON hierarchy
            SearchForOptions(json, optionsConfig, foundOptions);

            return foundOptions;
        }


        private void SearchForOptions(JToken token, ConfigurationMetadata config, List<TOptions> foundTools)
        {
            if (token is JObject obj)
            {
                // Check if this object has a "FieldType" property with the value "FieldMappingTool"
                if (obj.TryGetValue(config.ObjectName, out JToken fieldTypeToken) && fieldTypeToken.ToString() == config.OptionFor)
                {
                    // Deserialize the JObject into a FieldMappingToolOptions object
                    var options = obj.ToObject<TOptions>();
                    foundTools.Add(options);
                }

                // Recursively search child objects
                foreach (var property in obj.Properties())
                {
                    SearchForOptions(property.Value, config, foundTools);
                }
            }
            else if (token is JArray array)
            {
                // Recursively search elements in the array
                foreach (var item in array)
                {
                    SearchForOptions(item, config, foundTools);
                }
            }
        }

        public string CreateNewConfigurationJson(TOptions options, string path, string objectName, string optionFor, bool isCollection = false, bool shouldAddObjectName = false)
        {
            // Load existing configuration from a file or create a new JObject if necessary
            JObject configJson = new JObject();

            // Add or update the options in the configuration using the new method signature
            configJson = AddOptionsToConfiguration(configJson, options, path, objectName, optionFor, isCollection, shouldAddObjectName);

            // Return the updated JSON as a formatted string
            return configJson.ToString(Formatting.Indented);
        }

        // New method that updates the configuration
        public JObject AddOptionsToConfiguration(
      JObject configJson,
      TOptions options,
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

                        // Add the object name if required
                        if (shouldAddObjectName)
                        {
                            optionsObject.AddFirst(new JProperty(objectName, optionFor));
                        }

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






        private ConfigurationMetadata GetOptionsConfiguration()
        {
            TOptions options = new TOptions();
            return options.ConfigurationMetadata;
        }






    }
}
