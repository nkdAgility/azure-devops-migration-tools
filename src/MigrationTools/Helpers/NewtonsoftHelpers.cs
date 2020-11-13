using MigrationTools._EngineV1.Configuration;
using Newtonsoft.Json;

namespace MigrationTools.Helpers
{
    public static class NewtonsoftHelpers
    {
        public static string SerializeObject(object obj)
        {
            string json = JsonConvert.SerializeObject(obj, Formatting.Indented,
                    new FieldMapConfigJsonConverter(),
                        new ProcessorConfigJsonConverter(),
                        new MigrationClientConfigJsonConverter());
            return json;
        }

        public static T DeserializeObject<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json,
                new FieldMapConfigJsonConverter(),
                new ProcessorConfigJsonConverter(),
                new MigrationClientConfigJsonConverter());
        }
    }
}