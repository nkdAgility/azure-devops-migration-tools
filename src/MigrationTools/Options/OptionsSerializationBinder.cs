using System;
using System.Linq;
using Newtonsoft.Json.Serialization;
using Serilog;

namespace MigrationTools.Options
{
    public class OptionsSerializationBinder : ISerializationBinder
    {
        public void BindToName(Type serializedType, out string assemblyName, out string typeName)
        {
            assemblyName = null;
            typeName = serializedType.Name;
        }

        public Type BindToType(string assemblyName, string typeName)
        {
            Type type = AppDomain.CurrentDomain.GetAssemblies()
              .Where(a => !a.IsDynamic)
              .SelectMany(a => a.GetTypes())
              .FirstOrDefault(t => t.Name.Equals(typeName) || t.FullName.Equals(typeName));
            if (type is null || type.IsAbstract || type.IsInterface)
            {
                Log.Warning("Unable to load Processor: {typename}", typeName);
                throw new InvalidOperationException();
            }
            return type;
        }
    }
}