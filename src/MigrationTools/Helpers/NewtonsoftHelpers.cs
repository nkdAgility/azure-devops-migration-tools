using System.Collections.Generic;
using Microsoft.Extensions.Options;
using MigrationTools._EngineV1.Configuration;
using MigrationTools.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema.Generation;
using Newtonsoft.Json.Schema;

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

        public static IList<string> GetValidationResults<TObjectToValidateAgainst>(string jsonStringToValidate)
        {
            //validate schema
            JSchemaGenerator generator = new JSchemaGenerator();
            JSchema schema = generator.Generate(typeof(TObjectToValidateAgainst));
            
            var model = JObject.Parse(jsonStringToValidate);

            IList<string> messages;
            bool valid = model.IsValid(schema, out messages); // properly validates

            if (valid)
            {
                return null;
            } else
            {
                return messages;
            }

        }

    }
}