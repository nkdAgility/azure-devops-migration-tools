using MigrationTools._EngineV1.Configuration;
using MigrationTools.Endpoints;
using MigrationTools.Enrichers;
using Newtonsoft.Json;

namespace MigrationTools.Tests
{
    public static class TestHelpers
    {
        public static int SaveDocsObjectAsJSON(object obj)
        {
            string json = JsonConvert.SerializeObject(obj, Formatting.Indented,
                      new ProcessorConfigJsonConverter(),
                      new JsonConverterForEndpointOptions(),
                      new JsonConverterForEnricherOptions());
            System.IO.File.WriteAllText(string.Format("../../../../../docs/v2/Reference/JSON/{0}.json", obj.GetType().Name), json);
            return 0;
        }
    }
}