using MigrationTools._EngineV1.Configuration;
using MigrationTools.Endpoints;
using MigrationTools.Enrichers;
using Newtonsoft.Json;

namespace MigrationTools.Tests
{
    public static class TestHelpers
    {
        public static string SaveObjectAsJson(object obj)
        {
            string json = JsonConvert.SerializeObject(obj, Formatting.Indented,
                new ProcessorConfigJsonConverter(),
                      new ProcessorConfigJsonConverter(),
                      new JsonConverterForEndpointOptions(),
                      new IOptionsJsonConvertor());
            return json;
        }
    }
}