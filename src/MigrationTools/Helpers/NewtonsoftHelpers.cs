using System.Collections.Generic;
using MigrationTools._EngineV1.Configuration;
using Newtonsoft.Json;

namespace MigrationTools.Helpers
{
    public static class NewtonsoftHelpers
    {
        public static T DeserializeObject<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json, GetSerializerSettings());
        }

        public static string SerializeObject(object obj)
        {
            string json = JsonConvert.SerializeObject(obj, GetSerializerSettings());
            return json;
        }

        private static JsonSerializerSettings GetSerializerSettings()
        {
            return new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                Formatting = Formatting.Indented,
                Converters = new List<JsonConverter>() {
                    { new  FieldMapConfigJsonConverter() },
                     {  new ProcessorConfigJsonConverter() }
                    ,
                     {  new MigrationClientConfigJsonConverter() }
                }
            };
        }
    }
}