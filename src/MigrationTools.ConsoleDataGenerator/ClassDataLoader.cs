using Microsoft.Extensions.Configuration;
using MigrationTools.ConsoleDataGenerator.ReferenceData;
using MigrationTools.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;

namespace MigrationTools.ConsoleDataGenerator
{
    public class ClassDataLoader
    {
        private DataSerialization saveData;
        private readonly CodeDocumentation codeDocs;
        private readonly CodeFileFinder codeFinder;
        private IConfiguration configuration;
        public ClassDataLoader(string rootPath, DataSerialization saveData, Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            // XML documentation files are in the binary output directory
            string xmlDocsPath = AppDomain.CurrentDomain.BaseDirectory;
            codeDocs = new CodeDocumentation(xmlDocsPath);
            codeFinder = new CodeFileFinder(rootPath);
            this.saveData = saveData;
            this.configuration = configuration;
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
            var oConfig = GetOptionsConfiguration(optionInFocus);
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
                ///bind Item or Defaults
                if (!string.IsNullOrEmpty(instanceOfOption.ConfigurationMetadata.PathToDefault))
                {
                    IConfigurationSection mainOrDefaultSection;
                    mainOrDefaultSection = configuration.GetSection(instanceOfOption.ConfigurationMetadata.PathToDefault);
                    if (mainOrDefaultSection.Exists())
                    {
                        mainOrDefaultSection.Bind(instanceOfOption);
                        var json = ConvertSectionWithPathToJson(configuration, mainOrDefaultSection, instanceOfOption);
                        data.ConfigurationSamples.Add(new ConfigurationSample() { Name = "defaults", Order = 2, SampleFor = data.OptionsClassFullName, Code = json.Trim() });
                    }
                    else
                    {
                        data.ConfigurationSamples.Add(new ConfigurationSample() { Name = "defaults", Order = 2, SampleFor = data.OptionsClassFullName, Code = "There are no defaults! Check the sample for options!" });
                    }
                }
                if (!string.IsNullOrEmpty(instanceOfOption.ConfigurationMetadata.PathToSample))
                {
                    IConfigurationSection sampleSection = configuration.GetSection(instanceOfOption.ConfigurationMetadata.PathToSample);
                    sampleSection.Bind(instanceOfOption);
                    if (sampleSection.Exists())
                    {
                        var json = ConvertSectionWithPathToJson(configuration, sampleSection, instanceOfOption);
                        data.ConfigurationSamples.Add(new ConfigurationSample() { Name = "sample", Order = 1, SampleFor = data.OptionsClassFullName, Code = json.Trim() });
                    }
                    else
                    {
                        data.ConfigurationSamples.Add(new ConfigurationSample() { Name = "sample", Order = 1, SampleFor = data.OptionsClassFullName, Code = "There is no sample, but you can check the classic below for a general feel." });
                    }
                }
                data.ConfigurationSamples.Add(new ConfigurationSample() { Name = "classic", Order = 3, SampleFor = data.OptionsClassFullName, Code = saveData.SeraliseDataToJson(instanceOfOption).Trim() });
                if (instanceOfOption != null)
                {
                    JObject joptions = (JObject)JToken.FromObject(instanceOfOption);
                    data.Options = populateOptions(instanceOfOption, joptions);
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

            // Use reflection to get all public properties, not just those in the JSON serialization
            // This ensures properties with DefaultValueHandling.Ignore are still documented
            var properties = item.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                .Where(p => p.CanRead && p.CanWrite)
                .OrderBy(p => p.Name);

            foreach (var property in properties)
            {
                // Get the property value and handle complex objects appropriately for JProperty
                var propertyValue = GetSimplePropertyValue(property, item);

                // Create a temporary JProperty for compatibility with existing code documentation methods
                var tempJProperty = new JProperty(property.Name, propertyValue);

                OptionsItem optionsItem = new OptionsItem();
                optionsItem.ParameterName = property.Name;
                optionsItem.Type = property.PropertyType.Name.Replace("`1", "").Replace("`2", "").Replace("`", "");
                optionsItem.DotNetType = property.PropertyType;
                optionsItem.Description = codeDocs.GetPropertyData(item.GetType(), joptions, tempJProperty, "summary");
                optionsItem.DefaultValue = codeDocs.GetPropertyData(item.GetType(), joptions, tempJProperty, "default");
                // Determine if the property is required via DataAnnotations [Required]
                var isRequired = property.GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.RequiredAttribute), inherit: true).Any();
                optionsItem.IsRequired = isRequired;
                options.Add(optionsItem);
            }

            return options;
        }

        private object GetSimplePropertyValue(System.Reflection.PropertyInfo property, object instance)
        {
            try
            {
                var value = property.GetValue(instance);

                // Handle complex objects by returning a simple representation
                if (value == null)
                    return null;

                // For simple types, return the value as is
                if (property.PropertyType.IsPrimitive || property.PropertyType == typeof(string) || property.PropertyType == typeof(DateTime) || property.PropertyType.IsEnum)
                    return value;

                // For complex objects, return a simple string representation to avoid JSON serialization issues
                return value.ToString();
            }
            catch
            {
                // Return a default value for the property type if we can't get the actual value
                if (property.PropertyType == typeof(string))
                    return "";
                if (property.PropertyType == typeof(bool))
                    return false;
                if (property.PropertyType.IsValueType)
                    return Activator.CreateInstance(property.PropertyType);
                return null;
            }
        }

        static string ConvertSectionWithPathToJson(IConfiguration configuration, IConfigurationSection section, IOptions option = null)
        {
            var pathSegments = option == null ? section.Path.Split(':') : option.ConfigurationMetadata.PathToInstance.Split(':');

            // If IsKeyed is true, we skip the lowest path segment
            if (option != null && option.ConfigurationMetadata.IsKeyed && pathSegments.Length > 1)
            {
                pathSegments = pathSegments.Take(pathSegments.Length - 1).ToArray();
            }


            JObject root = new JObject();
            JObject currentObject = root;

            // Walk down the path from the root to the target section
            for (int i = 0; i < pathSegments.Length; i++)
            {
                string key = pathSegments[i];
                IConfigurationSection currentSection = configuration.GetSection(string.Join(":", pathSegments, 0, i + 1));

                if (i < pathSegments.Length - 1)
                {
                    // Create nested objects for intermediate segments
                    if (currentObject[key] == null)
                    {
                        currentObject[key] = new JObject();
                    }

                    // Add the additional property to the first section (top level in the path)
                    if (i == 0 && option != null)
                    {
                        ((JObject)currentObject[key]).Add("Version", "16.0");
                    }

                    currentObject = (JObject)currentObject[key];
                }
                else
                {
                    // We are at the target section, serialize its children
                    JToken sectionObject = ConvertSectionToJson(section);

                    if (option != null && option.ConfigurationMetadata.IsCollection)
                    {
                        // Handle as a collection
                        if (currentObject[key] == null)
                        {
                            currentObject[key] = new JArray();
                        }
                        if (currentObject[key] is JArray array)
                        {
                            JObject itemObject = sectionObject as JObject ?? new JObject();
                            if (!itemObject.ContainsKey(option.ConfigurationMetadata.ObjectName))
                            {
                                // Add ObjectName and OptionFor to the object
                                itemObject.AddFirst(new JProperty(option.ConfigurationMetadata.ObjectName, option.ConfigurationMetadata.OptionFor));
                            }

                            array.Add(itemObject);
                        }
                    }
                    else
                    {
                        // Handle as a regular object
                        JObject itemObject = sectionObject as JObject ?? new JObject();

                        // Add ObjectName and OptionFor if IsKeyed is true
                        if (option != null && option.ConfigurationMetadata.IsKeyed)
                        {
                            itemObject.AddFirst(new JProperty(option.ConfigurationMetadata.ObjectName, option.ConfigurationMetadata.OptionFor));
                        }

                        currentObject[key] = itemObject;
                    }
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


        public static ConfigurationMetadata GetOptionsConfiguration(Type option)
        {
            // ActivatorUtilities.CreateInstance(option);
            IOptions optionInsance = (IOptions)Activator.CreateInstance(option);
            return optionInsance.ConfigurationMetadata;
        }

    }
}
