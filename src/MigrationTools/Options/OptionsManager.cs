using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MigrationTools.Options
{
    public struct OptionsConfiguration
    {
        public string SectionPath { get; internal set; }
        public string CollectionPath { get; internal set; }
        public string CollectionObjectName { get; internal set; }
        public string OptionFor { get; internal set; }
    }

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

        public static OptionsConfiguration GetOptionsConfiguration(Type option)
        {
            dynamic optionInsance = Activator.CreateInstance(option);
            OptionsConfiguration oc = new OptionsConfiguration();
            oc.SectionPath = (string)option.GetProperty("ConfigurationSectionPath")?.GetValue(optionInsance);
            oc.CollectionPath = (string)option.GetProperty("ConfigurationCollectionPath")?.GetValue(optionInsance);
            oc.CollectionObjectName = (string)option.GetProperty("ConfigurationCollectionObjectName")?.GetValue(optionInsance); 
            oc.OptionFor = (string)option.GetProperty("ConfigurationOptionFor")?.GetValue(optionInsance);
            return oc;
        }

    }

    public class OptionsManager<TOptions> where TOptions : class, IOptions, new()
    {
        public TOptions LoadConfiguration(string filePath, bool isCollection = false)
        {
            var optionsConfig = GetOptionsConfiguration();
            JObject json = File.Exists(filePath) ? JObject.Parse(File.ReadAllText(filePath)) : new JObject();

            // Determine the path based on whether this is a collection or a section
            string path = isCollection ? optionsConfig.CollectionPath : optionsConfig.SectionPath;

            if (isCollection)
            {
                // Load from a collection
                var collection = json.SelectToken(path.Replace(":", ".")) as JArray;

                var item = collection?.FirstOrDefault(p => p[optionsConfig.CollectionObjectName]?.ToString() == optionsConfig.OptionFor);

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
            string path = isCollection ? options.ConfigurationCollectionPath : options.ConfigurationSectionPath;

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
                    var existingItem = collectionArray.FirstOrDefault(p => p[options.ConfigurationCollectionObjectName]?.ToString() == options.ConfigurationOptionFor);

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
                        newItem[options.ConfigurationCollectionObjectName] = options.ConfigurationOptionFor;
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


        private void SearchForOptions(JToken token, OptionsConfiguration config, List<TOptions> foundTools)
        {
            if (token is JObject obj)
            {
                // Check if this object has a "FieldType" property with the value "FieldMappingTool"
                if (obj.TryGetValue(config.CollectionObjectName, out JToken fieldTypeToken) && fieldTypeToken.ToString() == config.OptionFor)
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

        public string CreateNewConfigurationJson(TOptions options, bool isCollection = false)
        {
            // Create a new JObject to represent the entire configuration
            JObject newJson = new JObject();

            // Determine the path based on whether this is a collection or a section
            string path = isCollection ? options.ConfigurationCollectionPath : options.ConfigurationSectionPath;

            // Split the path into its components
            string[] pathParts = path.Split(':');
            JObject currentSection = newJson;

            // Build the JSON structure for the section or collection
            for (int i = 0; i < pathParts.Length; i++)
            {
                if (i == pathParts.Length - 1 && isCollection)
                {
                    // If it's a collection, create a JArray
                    if (currentSection[pathParts[i]] == null)
                    {
                        currentSection[pathParts[i]] = new JArray();
                    }

                    // Add the options object as part of the collection
                    var collectionArray = (JArray)currentSection[pathParts[i]];
                    var optionsObject = JObject.FromObject(options);
                    optionsObject[options.ConfigurationCollectionObjectName] = options.ConfigurationOptionFor;
                    collectionArray.Add(optionsObject);
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

            // If it's not a collection, add the options content directly to the final section
            if (!isCollection)
            {
                currentSection.Replace(JObject.FromObject(options));
            }

            // return the new JSON as a formatted string
            return newJson.ToString(Formatting.Indented);
        }


        private OptionsConfiguration GetOptionsConfiguration()
        {
            TOptions options = new TOptions();
            OptionsConfiguration oc = new OptionsConfiguration();
            oc.SectionPath = options.ConfigurationSectionPath;
            oc.CollectionPath = options.ConfigurationCollectionPath;
            oc.CollectionObjectName = options.ConfigurationCollectionObjectName;
            oc.OptionFor = options.ConfigurationOptionFor;
            return oc;
        }




        

    }
}
