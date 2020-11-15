using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace MigrationTools.Options
{
    public class OptionsSerializeContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);
            if (property.PropertyName == "RefName")
            {
                property.ShouldSerialize = instance => string.IsNullOrEmpty((instance?.GetType().GetProperty(property.PropertyName).GetValue(instance)?.ToString()));
            }
            return property;
        }
    }
}