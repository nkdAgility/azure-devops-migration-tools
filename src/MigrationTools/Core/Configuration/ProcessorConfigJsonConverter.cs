using MigrationTools.Core.Configuration.Processing;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MigrationTools.Core.Configuration
{
    public class ProcessorConfigJsonConverter : JsonCreationConverter<ITfsProcessingConfig>
    {
        protected override ITfsProcessingConfig Create(Type objectType, JObject jObject)
        {
            if (FieldExists("ObjectType", jObject))
            {
                string typename = jObject.GetValue("ObjectType").ToString();
                Type type = Type.GetType(typename, true);
                return (ITfsProcessingConfig)Activator.CreateInstance(type);
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


