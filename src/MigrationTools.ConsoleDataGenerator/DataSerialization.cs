using MigrationTools.ConsoleDataGenerator.ReferenceData;
using MigrationTools.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace MigrationTools.ConsoleDataGenerator
{
    public class DataSerialization
    {

        private readonly string dataPath;
        private readonly string referencePath;

        public DataSerialization(string rootPath)
        {
            dataPath = Path.Combine(rootPath, "docs/_data");
            referencePath = Path.Combine(rootPath, "docs/collections/_reference");
        }

        public string SeraliseData(DataItem data, string apiVersion, string dataTypeName)
        {
            string filename = $"reference.{apiVersion}.{dataTypeName}.{data.classData.ClassName}";
            string filePath = Path.Combine(dataPath, filename.ToLower());
            string yaml = SeraliseDataToYaml(data);
            File.WriteAllText($"{filePath}.yaml", yaml);
            yaml = "---" + '\n';
            yaml = yaml + SeraliseDataToYaml(data.classData) + '\n' + SeraliseDataToYaml(data.jekyllData);
            yaml = yaml + '\n' + "---";
            filePath = Path.Combine(referencePath, filename.ToLower());
            File.WriteAllText($"{filePath}.md", yaml);
            return yaml;
        }

        public string WriteYamlDataToDataFolder(ClassData data)
        {
            string filename = $"reference.{data.TypeName}.{data.ClassName}";
            string filePath = Path.Combine(dataPath, filename.ToLower());
            string yaml = SeraliseDataToYaml(data);
            File.WriteAllText($"{filePath}.yaml", yaml);
            return yaml;
        }

        public string WriteMarkdownDataToCollectionFolder(ClassData cdata, JekyllData jdata)
        {
            string filename = $"reference.{cdata.TypeName}.{cdata.ClassName}";
            string filePath = Path.Combine(referencePath, filename.ToLower());
            string yaml = "---" + '\n';
            yaml = yaml + SeraliseDataToYaml(cdata) + '\n';
            yaml = yaml + SeraliseDataToYaml(jdata) + '\n';
            yaml = yaml + "---";
            File.WriteAllText($"{filePath}.md", yaml);
            return yaml;
        }

        public string WriteHugoContentFile(ClassData cdata)
        {
            // Create Hugo-style content files that reference data
            string contentDir = Path.Combine(dataPath.Replace("_data", "content/reference"), cdata.TypeName.ToLower());
            Directory.CreateDirectory(contentDir);
            
            string filename = $"{cdata.ClassName.ToLower()}.md";
            string filePath = Path.Combine(contentDir, filename);
            
            string content = $@"---
title: ""{cdata.ClassName}""
type: ""reference""
layout: ""reference""
typeName: ""{cdata.TypeName}""
className: ""{cdata.ClassName}""
url: ""/Reference/{cdata.TypeName}/{cdata.ClassName}/""
aliases:
  - ""/Reference/{cdata.TypeName}/{cdata.OptionsClassName}/""
---

{{{{/* This page is generated from data in _data/{cdata.TypeName.ToLower()}.{cdata.ClassName.ToLower()}.yaml */}}}}
";
            
            File.WriteAllText(filePath, content);
            return content;
        }


        public string SeraliseData(ClassGroup data, string apiVersion, string dataTypeName)
        {
            string filename = $"reference.{apiVersion}.{dataTypeName}";
            string filePath = Path.Combine(dataPath, filename.ToLower());
            string json = SeraliseDataToJson(data);
            File.WriteAllText($"{filePath}.json", json);
            string yaml = SeraliseDataToYaml(data);
            File.WriteAllText($"{filePath}.yaml", yaml);
            return json;
        }

        public string SeraliseDataToJson(object data)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.None, // Disable automatic $type handling
                Formatting = Formatting.Indented,        // For better readability
                NullValueHandling = NullValueHandling.Ignore // Ignore null values
            };

            // Add our custom converter if a type is specified
            if (typeof(IOptions).IsAssignableFrom(data.GetType()))
            {

                settings.Converters.Add(new ConditionalTypeConverter(data.GetType()));
            }

            return JsonConvert.SerializeObject(data, settings);
        }

        public string SeraliseDataToYaml(object data)
        {
            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            return serializer.Serialize(data);
        }
    }

    public class ConditionalTypeConverter : JsonConverter
    {
        private readonly Type _typeToInclude;

        public ConditionalTypeConverter(Type typeToInclude)
        {
            _typeToInclude = typeToInclude;
        }

        public override bool CanConvert(Type objectType)
        {
            return true; // This converter applies to all types, but we'll decide whether to include $type inside WriteJson
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            JObject jo = JObject.FromObject(value);

            // Include $type only if the object is of the specified type
            if (value.GetType() == _typeToInclude)
            {
                jo.AddFirst(new JProperty("$type", value.GetType().Name));
            }

            jo.WriteTo(writer);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            // Deserialize normally
            return serializer.Deserialize(reader, objectType);
        }
    }

}
