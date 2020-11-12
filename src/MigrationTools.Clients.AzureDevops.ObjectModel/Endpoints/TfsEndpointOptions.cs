using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MigrationTools.Endpoints
{
    public enum AuthenticationMode
    {
        AccessToken = 0,
        Windows = 1,
        Prompt = 2
    }

    public class TfsEndpointOptions : EndpointOptions, ITfsEndpointOptions
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public AuthenticationMode AuthenticationMode { get; set; }

        public string AccessToken { get; set; }
        public string Organisation { get; set; }
        public string Project { get; set; }
        public string ReflectedWorkItemIdField { get; set; }

        public override Type ToConfigure => typeof(TfsEndpoint);

        public override void SetDefaults()
        {
            base.SetDefaults();
            AccessToken = "6i4jyylsadkjanjniaydxnjsi4zsz3qarxhl2y5ngzzffiqdostq";
            Organisation = "https://dev.azure.com/nkdagility-preview/";
            Project = "NeedToSetThis";
            ReflectedWorkItemIdField = "Custom.ReflectedWorkItemId";
        }
    }

    public interface ITfsEndpointOptions
    {
        public string AccessToken { get; }
        public string Organisation { get; }
        public string Project { get; }
        public string ReflectedWorkItemIdField { get; }
        public AuthenticationMode AuthenticationMode { get; }
    }
}