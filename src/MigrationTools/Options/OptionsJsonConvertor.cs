using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Serilog;

namespace MigrationTools.Options
{
    public abstract class OptionsJsonConvertor : JsonConverter
    {
        protected IOptions Create(Type objectType, JObject jObject)
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

                return (IOptions)Activator.CreateInstance(type);
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

        public override object ReadJson(JsonReader reader,
                                        Type objectType,
                                         object existingValue,
                                         JsonSerializer serializer)
        {
            // Load JObject from stream
            JObject jObject = JObject.Load(reader);

            // Create target object based on JObject
            IOptions target = Create(objectType, jObject);

            // Populate the object properties
            serializer.Populate(jObject.CreateReader(), target);

            return target;
        }

        public override void WriteJson(JsonWriter writer,
                                       object value,
                                       JsonSerializer serializer)
        {
            Log.Verbose("MigrationToolsJsonConvertor::WriteJson({ObjectName})", value.GetType().Name);

            JToken jt = JToken.FromObject(value);

            if (jt.Type != JTokenType.Object)
            {
                jt.WriteTo(writer);
            }
            else
            {
                JObject o = (JObject)jt;
                o.AddFirst(new JProperty("ObjectType", value.GetType().Name));
                o.WriteTo(writer);
            }

            //JObject jo = new JObject();
            //Type type = value.GetType();
            //jo.Add("ObjectType", type.Name);
            //foreach (PropertyInfo prop in type.GetProperties().Where(p => p.CanRead && p.CanWrite))
            //{
            //    if (prop.Name == "ToConfigure")
            //    {
            //        Log.Verbose("Moo");
            //    }
            //    if (prop.GetCustomAttributes(typeof(JsonIgnoreAttribute), true).Count() > 0)
            //    {
            //        Log.Verbose("MigrationToolsJsonConvertor::WriteJson: Ignore Property {Name} : {@CustomAttributes}", prop.Name, prop.GetCustomAttributes(true));
            //        continue;
            //    }
            //    if (prop.CanRead)
            //    {
            //        object propVal = prop.GetValue(value, null);
            //        if (propVal != null)
            //        {
            //            jo.Add(prop.Name, JToken.FromObject(propVal, serializer));
            //        }
            //    }
            //}

            //JToken jt = JToken.FromObject(value);
            //if (jt.Type != JTokenType.Object)
            //{
            //    jt.WriteTo(writer);
            //}
            //else
            //{
            //    JObject o = (JObject)jt;
            //    o.AddFirst(new JProperty("ObjectType", value.GetType().Name));
            //    o.WriteTo(writer);
            //}
            //jo.WriteTo(writer);
        }
    }
}