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

        [Obsolete("Please use GetClassData instead")]
        public ClassGroup GetClassGroup(List<Type> targetTypes, List<Type> allTypes, Type type, string apiVersion, string dataTypeName, bool findConfig = true, string configEnd = "Options")
        {
            Console.WriteLine();
            Console.WriteLine($"ClassDataLoader::BuildJekyllDataFile:: {dataTypeName}");
            ClassGroup data = new ClassGroup();
            data.Name = dataTypeName;
            var founds = targetTypes.Where(t => type.IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface && t.IsPublic).OrderBy(t => t.Name).ToList();
            Console.WriteLine($"ClassDataLoader::BuildJekyllDataFile:: ----------- Found {founds.Count}");

            // Each File
            foreach (var item in founds)
            {
                DataItem dataItem = new DataItem();

                Console.WriteLine($"ClassDataLoader::BuildJekyllDataFile::-PROCESS {item.Name}");
                dataItem.classData = CreateClassData(targetTypes, allTypes, apiVersion, dataTypeName, item, findConfig, configEnd);
            }
            Console.WriteLine("ClassDataLoader::BuildJekyllDataFile:: -----------");
            return data;
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
                data.Add(CreateClassData(targetTypes, allTypes, apiVersion, dataTypeName, item, findConfig, configEnd));
            }
            Console.WriteLine("ClassDataLoader::populateClassData:: -----------");
            return data;
        }

        private ClassData CreateClassData(List<Type> targetTypes, List<Type> allTypes, string apiVersion, string dataTypeName, Type item, bool findConfig = true, string configEnd = "Options")
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
                object targetItem = null;
                var ConfigurationSectionName = ((IOptions)typeOption).ConfigurationSectionName;
                if (!string.IsNullOrEmpty(ConfigurationSectionName))
                {
                    Console.WriteLine("Processing as ConfigurationSectionName");
                    var section = configuration.GetSection(ConfigurationSectionName);
                    targetItem = (IOptions)Activator.CreateInstance(typeOption);
                    section.Bind(targetItem);
                    data.ConfigurationSamples.Add(new ConfigurationSample() { Name = "default", SampleFor = data.OptionsClassFullName, Code = ConvertSectionWithPathToJson(configuration, section).Trim() } );
                }
                if (typeOption.GetInterfaces().Contains(typeof(IOptions)))
                {
                    Console.WriteLine("Processing as IOptions");
                    var options = (IOldOptions)Activator.CreateInstance(typeOption);
                    targetItem = options;
                }
                if (typeOption.GetInterfaces().Contains(typeof(IFieldMapOptions)))
                {
                    Console.WriteLine("Processing as IFieldMapConfig");
                    var options = (IFieldMapOptions)Activator.CreateInstance(typeOption);
                    options.SetExampleConfigDefaults();
                    targetItem = options;
                }
                if (targetItem != null)
                {
                    Console.WriteLine("targetItem");
                    JObject joptions = (JObject)JToken.FromObject(targetItem);
                    data.Options = populateOptions(targetItem, joptions);
                    data.ConfigurationSamples.Add(new ConfigurationSample() { Name = "Classic", SampleFor = data.OptionsClassFullName, Code = saveData.SeraliseDataToJson(targetItem).Trim() });
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

                JObject parentObject = new JObject();

                if (i == pathSegments.Length - 1)
                {
                    // We are at the target section, so only serialize this section
                    foreach (var child in currentSection.GetChildren())
                    {
                        if (child.Value != null)
                        {
                            parentObject[child.Key] = child.Value;
                        }
                        else
                        {
                            parentObject[child.Key] = ConvertSectionToJson(child);
                        }
                    }
                }

                currentObject[key] = parentObject;
                currentObject = parentObject;
            }

            return root.ToString(Formatting.Indented);
        }

        static JObject ConvertSectionToJson(IConfigurationSection section)
        {
            var jObject = new JObject();

            foreach (var child in section.GetChildren())
            {
                if (child.Value != null)
                {
                    jObject[child.Key] = child.Value;
                }
                else
                {
                    jObject[child.Key] = ConvertSectionToJson(child);
                }
            }

            return jObject;
        }

    }
}
