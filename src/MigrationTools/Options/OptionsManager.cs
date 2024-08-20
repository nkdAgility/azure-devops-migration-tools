using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MigrationTools.Options
{
    public class OptionsManager
    {
        public static dynamic GetOptionsManager(Type option, string filePath, string sectionPath, string optionFor, string sectionListPath, string objectTypeFieldName = "ObjectType")
        {
            Type optionsManagerType = typeof(OptionsManager<>);
            Type specificOptionsManagerType = optionsManagerType.MakeGenericType(option);

            object optionsManagerInstance = Activator.CreateInstance(
                specificOptionsManagerType,
                filePath,
                sectionPath,
                optionFor,
                sectionListPath,
                objectTypeFieldName
            );
            return optionsManagerInstance;
        }
    }



    public class OptionsManager<TOptions> where TOptions : class, IOptions, new()
    {
        private readonly string _filePath;

        public OptionsManager(string filePath)
        {
            _filePath = filePath;
        }       

        public TOptions LoadFromConfigurationSection()
        {
            var optionsConfig = GetOptionsConfiguration();
            JObject json = File.Exists(_filePath) ? JObject.Parse(File.ReadAllText(_filePath)) : new JObject();
            var section = json.SelectToken(optionsConfig.SectionPath.Replace(":", "."));

            return section != null
                ? section.ToObject<TOptions>()
                : new TOptions();
        }

        public void SaveToConfigurationSection(TOptions options)
        {
            JObject json = File.Exists(_filePath) ? JObject.Parse(File.ReadAllText(_filePath)) : new JObject();
            JObject currentSection = json;

            string[] sections = options.ConfigurationSectionPath.Split(':');
            foreach (var section in sections)
            {
                if (currentSection[section] == null)
                {
                    currentSection[section] = new JObject();
                }

                currentSection = (JObject)currentSection[section];
            }

            currentSection.Replace(JObject.FromObject(options));
            File.WriteAllText(_filePath, json.ToString(Formatting.Indented));
        }

        public TOptions LoadFromSectionListPath()
        {
            var optionsConfig = GetOptionsConfiguration();
            JObject json = File.Exists(_filePath) ? JObject.Parse(File.ReadAllText(_filePath)) : new JObject();
            var processors = json.SelectToken(optionsConfig.CollectionPath.Replace(":", ".")) as JArray;

            var processor = processors?.FirstOrDefault(p => p[optionsConfig.CollectionObjectName]?.ToString() == optionsConfig.OptionFor);

            return processor != null
                ? processor.ToObject<TOptions>()
                : new TOptions();
        }

        public void SaveToSectionListPath(TOptions options)
        {
            JObject json = File.Exists(_filePath) ? JObject.Parse(File.ReadAllText(_filePath)) : new JObject();
            var processors = json.SelectToken(options.ConfigurationCollectionPath) as JArray ?? new JArray();

            var processor = JObject.FromObject(options);
            processor[options.ConfigurationCollectionObjectName] = options.ConfigurationOptionFor;

            processors.Add(processor);

            // Create the path to the Processors array in the JSON structure
            string[] sections = options.ConfigurationCollectionPath.Split(':');
            JObject currentSection = json;
            for (int i = 0; i < sections.Length - 1; i++)
            {
                if (currentSection[sections[i]] == null)
                {
                    currentSection[sections[i]] = new JObject();
                }

                currentSection = (JObject)currentSection[sections[i]];
            }

            // Access the last section using the length of the array
            currentSection[sections[sections.Length - 1]] = processors;

            File.WriteAllText(_filePath, json.ToString(Formatting.Indented));
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

        public struct OptionsConfiguration
        {
            public string SectionPath { get; internal set; }
            public string CollectionPath { get; internal set; }
            public string CollectionObjectName { get; internal set; }
            public string OptionFor { get; internal set; }
        }

    }
}
