using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MigrationTools._EngineV1.Configuration;
using MigrationTools.ConsoleDataGenerator.ReferenceData;
using MigrationTools.Options;
using Newtonsoft.Json.Linq;
using MigrationTools;
using System.Configuration;
using Newtonsoft.Json;
using MigrationTools.Tools.Infrastructure;
using System.Security.AccessControl;
using Microsoft.Extensions.Options;
using MigrationTools.Processors;

namespace MigrationTools.ConsoleDataGenerator
{
    public class ClassDataLoader
    {
        private DataSerialization saveData;
        private static CodeDocumentation codeDocs = new CodeDocumentation("../../../../../docs/Reference/Generated/");
        private static CodeFileFinder codeFinder = new CodeFileFinder("../../../../../src/");
        private IConfiguration configuration;
        public ClassDataLoader(DataSerialization saveData, Microsoft.Extensions.Configuration.IConfiguration configuration)
        {

            this.saveData = saveData;
            this.configuration = configuration;
        }

        public List<ClassData> GetClassData(List<Type> targetTypes, List<Type> allTypes, Type type, string apiVersion, string dataTypeName, bool findConfig = true, string configEnd = "Options")
        {
            Console.WriteLine();
            Console.WriteLine($"ClassDataLoader::populateClassData:: {dataTypeName}");
            List<ClassData> data = new List<ClassData>();
            var founds = targetTypes.Where(t => type.IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface && t.IsPublic).OrderBy(t => t.Name).ToList();
            Console.WriteLine($"ClassDataLoader::populateClassData:: ----------- Found {founds.Count}");
            // Each File
            foreach (var item in founds)
            {
                Console.WriteLine($"ClassDataLoader::populateClassData::-PROCESS {item.Name}");
                data.Add(CreateClassData(targetTypes, allTypes, type, apiVersion, dataTypeName, item, findConfig, configEnd));
            }
            Console.WriteLine("ClassDataLoader::populateClassData:: -----------");
            return data;
        }

        public List<ClassData> GetClassDataFromOptions<TOptionsInterface>(List<Type> allTypes, string dataTypeName)
            where TOptionsInterface : IOptions
        {
            Console.WriteLine();
            Console.WriteLine($"ClassDataLoader::GetOptionsData:: {dataTypeName}");
            List<ClassData> data = new List<ClassData>();
            var founds = allTypes.Where(t => typeof(TOptionsInterface).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface && t.IsPublic).OrderBy(t => t.Name).ToList();
            Console.WriteLine($"ClassDataLoader::GetOptionsData:: ----------- Found {founds.Count}");
            // Each File
            foreach (var item in founds)
            {
                Console.WriteLine($"ClassDataLoader::CreateClassDataFromOptions::-PROCESS {item.Name}");
                var itemData = CreateClassDataFromOptions<TOptionsInterface>(allTypes, dataTypeName, item);
                if (itemData != null)
                {
                    data.Add(itemData);
                }
                else
                {
                    Console.WriteLine($"BOOM::CreateClassDataFromOptions");
                }

            }
            Console.WriteLine("ClassDataLoader::GetOptionsData:: -----------");
            return data;
        }

        private ClassData CreateClassDataFromOptions<TOptionsInterface>(List<Type> allTypes, string dataTypeName, Type optionInFocus)
            where TOptionsInterface : IOptions
        {
           var oConfig =  OptionsManager.GetOptionsConfiguration(optionInFocus);
            var typeOftargetOfOption = allTypes.Where(t => t.Name == oConfig.OptionFor && !t.IsAbstract && !t.IsInterface).SingleOrDefault();
            if (typeOftargetOfOption == null)
            {
                Console.WriteLine($"ClassDataLoader::CreateClassDataFromOptions:: {optionInFocus.Name} - {oConfig.OptionFor} not found");
                return null;
            }
            ClassData data = new ClassData();
            data.ClassName = typeOftargetOfOption.Name;
            data.ClassFile = codeFinder.FindCodeFile(typeOftargetOfOption);
            data.TypeName = dataTypeName;
            data.Description = codeDocs.GetTypeData(typeOftargetOfOption);
            data.Status = codeDocs.GetTypeData(typeOftargetOfOption, "status");
            data.ProcessingTarget = codeDocs.GetTypeData(typeOftargetOfOption, "processingtarget");

            if (optionInFocus != null)
            {
                TOptionsInterface instanceOfOption = (TOptionsInterface)Activator.CreateInstance(optionInFocus);

                data.OptionsClassFullName = optionInFocus.FullName;
                data.OptionsClassName = optionInFocus.Name;
                data.OptionsClassFile = codeFinder.FindCodeFile(optionInFocus);
                if (!string.IsNullOrEmpty(oConfig.SectionPath))
                {
                    Console.WriteLine("Processing as ConfigurationSectionName");
                    var section = configuration.GetSection(oConfig.SectionPath);
                    section.Bind(instanceOfOption);

                    string jsonCollection = Options.OptionsManager.CreateNewConfigurationJson(instanceOfOption, !string.IsNullOrEmpty(instanceOfOption.ConfigurationCollectionPath));

                    data.ConfigurationSamples.Add(new ConfigurationSample() { Name = "confinguration.json", SampleFor = data.OptionsClassFullName, Code = jsonCollection });
                    data.ConfigurationSamples.Add(new ConfigurationSample() { Name = "defaults", SampleFor = data.OptionsClassFullName, Code = ConvertSectionWithPathToJson(configuration, section).Trim() });

                }

                Console.WriteLine("targetItem");
                JObject joptions = (JObject)JToken.FromObject(instanceOfOption);
                data.Options = populateOptions(instanceOfOption, joptions);
                data.ConfigurationSamples.Add(new ConfigurationSample() { Name = "Classic", SampleFor = data.OptionsClassFullName, Code = saveData.SeraliseDataToJson(instanceOfOption).Trim() });
            }
            else
            {

            }
            return data;
        }


        private ClassData CreateClassData(List<Type> targetTypes, List<Type> allTypes, Type type, string apiVersion, string dataTypeName, Type item, bool findConfig = true, string configEnd = "Options")
        {
            Type typeOption = item;
            string objectName = item.Name;
            ClassData data = new ClassData();
            data.ClassName = item.Name;
            data.ClassFile = codeFinder.FindCodeFile(item);
            data.TypeName = dataTypeName;
            data.Architecture = apiVersion;
            data.Description = codeDocs.GetTypeData(item);
            data.Status = codeDocs.GetTypeData(item, "status");
            data.ProcessingTarget = codeDocs.GetTypeData(item, "processingtarget");
            if (findConfig)
            {
                objectName = objectName.Replace("Context", "");
                typeOption = allTypes.Where(t => t.Name == $"{objectName}{configEnd}" && !t.IsAbstract && !t.IsInterface).SingleOrDefault();

            }
            else
            {
                data.OptionsClassName = "";
                data.OptionsClassFullName = "";
                Console.WriteLine("No config");
            }

            if (typeOption != null)
            {
                data.OptionsClassFullName = typeOption.FullName;
                data.OptionsClassName = typeOption.Name;
                data.OptionsClassFile = codeFinder.FindCodeFile(typeOption);
                var instanceOfOptions = Activator.CreateInstance(typeOption);
               
                ///bind Item or Defaults
                var ConfigurationSectionName = (string)typeOption.GetProperty("ConfigurationSectionName")?.GetValue(instanceOfOptions);
                if (!string.IsNullOrEmpty(ConfigurationSectionName))
                {
                    IConfigurationSection mainOrDefaultSection;
                    Console.WriteLine("Processing as ConfigurationSectionName");
                    mainOrDefaultSection = configuration.GetSection(ConfigurationSectionName);
                    mainOrDefaultSection.Bind(instanceOfOptions);
                    data.ConfigurationSamples.Add(new ConfigurationSample() { Name = "defaults", SampleFor = data.OptionsClassFullName, Code = ConvertSectionWithPathToJson(configuration, mainOrDefaultSection).Trim() });
                }
                var ConfigurationSampleName = (string)typeOption.GetProperty("ConfigurationSampleName")?.GetValue(instanceOfOptions);
                if (!string.IsNullOrEmpty(ConfigurationSampleName))
                {
                    Console.WriteLine("targetItem");
                    IConfigurationSection sampleSection = configuration.GetSection(ConfigurationSampleName);
                    sampleSection.Bind(instanceOfOptions);
                    data.ConfigurationSamples.Add(new ConfigurationSample() { Name = "sample", SampleFor = data.OptionsClassFullName, Code = ConvertSectionWithPathToJson(configuration, sampleSection).Trim() });
                    data.ConfigurationSamples.Add(new ConfigurationSample() { Name = "classic", SampleFor = data.OptionsClassFullName, Code = saveData.SeraliseDataToJson(instanceOfOptions).Trim() });
                }
                if (instanceOfOptions != null)
                {
                    JObject joptions = (JObject)JToken.FromObject(instanceOfOptions);
                    data.Options = populateOptions(instanceOfOptions, joptions);
                }

            }
            else
            {

            }
            return data;
        }


        private List<OptionsItem> populateOptions(object item, JObject joptions)
        {
            List<OptionsItem> options = new List<OptionsItem>();
            if (!(joptions is null))
            {
                var jpropertys = joptions.Properties().OrderBy(t => t.Name);
                foreach (JProperty jproperty in jpropertys)
                {
                    OptionsItem optionsItem = new OptionsItem();
                    optionsItem.ParameterName = jproperty.Name;
                    optionsItem.Type = codeDocs.GetPropertyType(item, jproperty);
                    optionsItem.Description = codeDocs.GetPropertyData(item, joptions, jproperty, "summary");
                    optionsItem.DefaultValue = codeDocs.GetPropertyData(item, joptions, jproperty, "default");
                    options.Add(optionsItem);
                }
            }
            return options;
        }

        static string ConvertSectionWithPathToJson(IConfiguration configuration, IConfigurationSection section)
        {
            var pathSegments = section.Path.Split(':');
            JObject root = new JObject();
            JObject currentObject = root;

            // Walk down the path from the root to the target section
            for (int i = 0; i < pathSegments.Length; i++)
            {
                string key = pathSegments[i];
                IConfigurationSection currentSection = configuration.GetSection(string.Join(':', pathSegments, 0, i + 1));

                if (i < pathSegments.Length - 1)
                {
                    // Create nested objects for intermediate segments
                    if (currentObject[key] == null)
                    {
                        currentObject[key] = new JObject();
                    }
                    currentObject = (JObject)currentObject[key];
                }
                else
                {
                    // We are at the target section, serialize its children
                    JToken sectionObject = ConvertSectionToJson(currentSection);
                    currentObject[key] = sectionObject;
                }
            }

            return root.ToString(Formatting.Indented);
        }

        static JToken ConvertSectionToJson(IConfigurationSection section)
        {
            // Check if all children are numbers to identify arrays
            var children = section.GetChildren().ToList();
            if (children.All(c => int.TryParse(c.Key, out _)))
            {
                // Treat it as an array
                JArray array = new JArray();
                foreach (var child in children)
                {
                    if (child.GetChildren().Any())
                    {
                        // Add nested objects to the array
                        array.Add(ConvertSectionToJson(child));
                    }
                    else
                    {
                        // Add values to the array
                        array.Add(child.Value);
                    }
                }
                return array;
            }
            else
            {
                // Treat it as an object
                JObject obj = new JObject();
                foreach (var child in children)
                {
                    if (child.GetChildren().Any())
                    {
                        // Recursively process nested objects
                        obj[child.Key] = ConvertSectionToJson(child);
                    }
                    else
                    {
                        // Add values to the object
                        obj[child.Key] = child.Value;
                    }
                }
                return obj;
            }
        }


    }
}
