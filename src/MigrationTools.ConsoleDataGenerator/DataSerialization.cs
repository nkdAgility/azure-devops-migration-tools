using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MigrationTools.Helpers;
using Newtonsoft.Json;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;

namespace MigrationTools.ConsoleDataGenerator
{
    public class DataSerialization
    {

        private static string dataPath = "../../../../../docs/_data/";
        public DataSerialization(string saveDataTo) {
            dataPath = saveDataTo;
        }

        public string SeraliseData(object data, string apiVersion, string dataTypeName)
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
            return NewtonsoftHelpers.SerializeObject(data, TypeNameHandling.Objects);
        }

        public string SeraliseDataToYaml(object data)
        {
            var serializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            return serializer.Serialize(data);
        }
    }
}
