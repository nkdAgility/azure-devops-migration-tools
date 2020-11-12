using System;
using System.Linq;
using MigrationTools.Options;
using Newtonsoft.Json.Linq;

namespace MigrationTools._EngineV1.Configuration
{
    public class FieldMapConfigJsonConverter : JsonConvertor<IFieldMapConfig>
    {
        protected override IFieldMapConfig Create(Type objectType, JObject jObject)
        {
            if (FieldExists("ObjectType", jObject))
            {
                string typename = jObject.GetValue("ObjectType").ToString();
                Type type = AppDomain.CurrentDomain.GetAssemblies()
                   .Where(a => !a.IsDynamic)
                   .SelectMany(a => a.GetTypes())
                   .FirstOrDefault(t => t.Name.Equals(typename) || t.FullName.Equals(typename));
                return (IFieldMapConfig)Activator.CreateInstance(type);
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