using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureDevOpsMigrationTools.Core.Configuration.FieldMap
{
    public class FieldMapConfigJsonConverter : JsonCreationConverter<IFieldMapConfig>
    {
        protected override IFieldMapConfig Create(Type objectType, JObject jObject)
        {
            if (FieldExists("ObjectType", jObject))
            {
                string typename = jObject.GetValue("ObjectType").ToString();
                Type type = Type.GetType(typename, true);
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


