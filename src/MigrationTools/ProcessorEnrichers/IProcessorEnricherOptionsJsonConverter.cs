using System;
using System.Linq;
using MigrationTools.Options;
using Newtonsoft.Json.Linq;
using Serilog;

namespace MigrationTools.Enrichers
{
    public class IProcessorEnricherOptionsJsonConverter : JsonOptionConvertor<IProcessorEnricherOptions>
    {
        protected override IProcessorEnricherOptions Create(Type objectType, JObject jObject)
        {
            if (FieldExists("ObjectType", jObject))
            {
                string typename = jObject.GetValue("ObjectType").ToString();
                Type type = AppDomain.CurrentDomain.GetAssemblies()
                  .Where(a => !a.IsDynamic)
                  .SelectMany(a => a.GetTypes())
                  .FirstOrDefault(t => t.Name.Equals(typename) || t.FullName.Equals(typename));
                if (type is null || type.IsAbstract || type.IsInterface)
                {
                    Log.Warning("Unable to load Processor: {typename}", typename);
                    throw new InvalidOperationException();
                }

                return (IProcessorEnricherOptions)Activator.CreateInstance(type);
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