using MigrationTools.Options;
using Newtonsoft.Json;

namespace MigrationTools.Helpers
{
    public static class NewtonsoftHelpers
    {
        public static T DeserializeObject<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json, GetSerializerSettings());
        }

        public static string SerializeObject(object obj, TypeNameHandling typeHandling = TypeNameHandling.Auto)
        {
            string json = JsonConvert.SerializeObject(obj, GetSerializerSettings(typeHandling));
            return json;
        }

        private static JsonSerializerSettings GetSerializerSettings(TypeNameHandling typeHandling = TypeNameHandling.Auto)
        {
            return new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                TypeNameHandling = typeHandling,
                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
                SerializationBinder = new OptionsSerializationBinder(),
                Formatting = Formatting.Indented,
                ContractResolver = new OptionsSerializeContractResolver()
            };
        }
    }
}