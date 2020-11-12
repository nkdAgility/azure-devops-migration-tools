using System;
using System.Linq;
using MigrationTools.Options;
using Newtonsoft.Json.Linq;
using Serilog;

namespace MigrationTools._EngineV1.Configuration
{
    public class ProcessorConfigJsonConverter : JsonConvertor<IProcessorConfig>
    {
        protected override IProcessorConfig Create(Type objectType, JObject jObject)
        {
            if (FieldExists("ObjectType", jObject))
            {
                string typename = jObject.GetValue("ObjectType").ToString();
                if (!typename.EndsWith("Options") && !typename.EndsWith("Config"))
                {
                    typename = string.Format("{0}Options", typename);
                }
                Type type = AppDomain.CurrentDomain.GetAssemblies()
                  .Where(a => !a.IsDynamic)
                  .SelectMany(a => a.GetTypes())
                  .FirstOrDefault(t => t.Name.Equals(typename) || t.FullName.Equals(typename));
                if (type is null || type.IsAbstract || type.IsInterface)
                {
                    Log.Warning("Unable to load Processor: {typename}", typename);
                    throw new InvalidOperationException();
                }
                return (IProcessorConfig)Activator.CreateInstance(type);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private bool FieldExists(string fieldName, JObject jObject)
        {
            return jObject[fieldName] != null;
        }
    }
}