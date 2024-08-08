using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using MigrationTools._EngineV1.Configuration;

namespace MigrationTools.Options
{


    public class ProcessorOptionsConverter : JsonConverter<IProcessorConfig>
    {
        public override IProcessorConfig Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var jsonDocument = JsonDocument.ParseValue(ref reader);
            var rootElement = jsonDocument.RootElement;
            var processorType = rootElement.GetProperty("ProcessorType").GetString();
            var prossessorOptionsType = GetProcessorFromTypeString(processorType);
            return JsonSerializer.Deserialize(rootElement.GetRawText(), prossessorOptionsType, options) as IProcessorConfig;
        }

        public override void Write(Utf8JsonWriter writer, IProcessorConfig value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString("ProcessorType", value.GetType().Name.Replace("Options", "").Replace("Config", ""));
            JsonSerializer.Serialize(writer, value, value.GetType(), options);
            writer.WriteEndObject();
        }

        private Type GetProcessorFromTypeString( string processorType)
        {
            // Get all loaded assemblies in the current application domain
           
            // Get all types from each assembly
            IEnumerable<Type> prosserOptionTypes = GetTypesImplementingInterface<IProcessorConfig>();
            return prosserOptionTypes.SingleOrDefault(type => type.Name.StartsWith(processorType));
        }

        private static IEnumerable<Type> GetTypesImplementingInterface<TInterface>()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(ass => (ass.FullName.StartsWith("MigrationTools") || ass.FullName.StartsWith("VstsSyncMigrator")));
            var interfaceType = typeof(TInterface);
            return  assemblies
                        .SelectMany(assembly => assembly.GetTypes())
                        .Where(type => interfaceType.IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract);
        }
    }

}
