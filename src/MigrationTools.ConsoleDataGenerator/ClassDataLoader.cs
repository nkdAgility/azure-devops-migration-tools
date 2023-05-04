using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MigrationTools._EngineV1.Configuration;
using MigrationTools.ConsoleDataGenerator.ReferenceData;
using MigrationTools.Options;
using Newtonsoft.Json.Linq;

namespace MigrationTools.ConsoleDataGenerator
{
    public class ClassDataLoader
    {
        private DataSerialization saveData;
        private static CodeDocumentation codeDocs = new CodeDocumentation("../../../../../docs/Reference/Generated/");
        public ClassDataLoader(DataSerialization saveData) {
            this.saveData = saveData;
        }

        public ClassGroup CreateClassGroup(List<Type> targetTypes, List<Type> allTypes, Type type, string apiVersion, string dataTypeName, bool findConfig = true, string configEnd = "Options")
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
                Console.WriteLine($"ClassDataLoader::BuildJekyllDataFile::-PROCESS {item.Name}");
                PopulateReferenceData(ref data, targetTypes, allTypes, apiVersion, dataTypeName, item, findConfig, configEnd);
            }
            Console.WriteLine("ClassDataLoader::BuildJekyllDataFile:: -----------");
            return data;
        }

        private void PopulateReferenceData(ref ClassGroup data, List<Type> targetTypes, List<Type> allTypes, string apiVersion, string dataTypeName, Type item, bool findConfig = true, string configEnd = "Options")
        {
            Type typeOption = item;
            string objectName = item.Name;
            DataItem dataItem = new DataItem();
            dataItem.classData.ClassName = item.Name;
            dataItem.classData.TypeName = dataTypeName;
            dataItem.classData.Architecture = apiVersion;

            if (findConfig)
            {
                objectName = objectName.Replace("Context", "");
                typeOption = allTypes.Where(t => t.Name == $"{objectName}{configEnd}" && !t.IsAbstract && !t.IsInterface).SingleOrDefault();

            }
            else
            {
                dataItem.classData.OptionsClassName = "";
                dataItem.classData.OptionsClassFullName = "";
                Console.WriteLine("No config");
            }


            if (typeOption != null)
            {
                dataItem.classData.OptionsClassFullName = typeOption.FullName;
                dataItem.classData.OptionsClassName = typeOption.Name;
                object targetItem = null;
                if (typeOption.GetInterfaces().Contains(typeof(IProcessorConfig)))
                {
                    Console.WriteLine("Processing as IProcessorConfig");
                    var options = (IProcessorConfig)Activator.CreateInstance(typeOption);
                    targetItem = options;
                }
                if (typeOption.GetInterfaces().Contains(typeof(IOptions)))
                {
                    Console.WriteLine("Processing as IOptions");
                    var options = (IOptions)Activator.CreateInstance(typeOption);
                    options.SetDefaults();
                    targetItem = options;
                }
                if (typeOption.GetInterfaces().Contains(typeof(IFieldMapConfig)))
                {
                    Console.WriteLine("Processing as IFieldMapConfig");
                    var options = (IFieldMapConfig)Activator.CreateInstance(typeOption);
                    options.SetExampleConfigDefaults();
                    targetItem = options;
                }
                if (targetItem != null)
                {
                    Console.WriteLine("targetItem");
                    JObject joptions = (JObject)JToken.FromObject(targetItem);

                    dataItem.classData.Options = populateOptions(targetItem, joptions);
                    dataItem.classData.ConfigurationSamples.Add(new ConfigurationSample() { Name = "default", SampleFor = dataItem.classData.OptionsClassFullName, Sample = saveData.SeraliseDataToJson(targetItem) });
                }

            }
            else
            {

            }
            dataItem.classData.Description = codeDocs.GetTypeData(item);
            data.Items.Add(dataItem);
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

    }
}
