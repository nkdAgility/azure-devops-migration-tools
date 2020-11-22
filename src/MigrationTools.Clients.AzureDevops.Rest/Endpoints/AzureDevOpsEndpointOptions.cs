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

    public class AzureDevOpsEndpointOptions : EndpointOptions, IAzureDevOpsEndpointOptions
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public AuthenticationMode AuthenticationMode { get; set; }

        public string AccessToken { get; set; }

        [JsonProperty(Order = -3)]
        public string Organisation { get; set; }

        [JsonProperty(Order = -2)]
        public string Project { get; set; }

        [JsonProperty(Order = -1)]
        public string ReflectedWorkItemIdField { get; set; }

        public override Type ToConfigure => typeof(AzureDevOpsEndpoint);

        public override void SetDefaults()
        {
            base.SetDefaults();
            AccessToken = "6i4jyylsadkjanjniaydxnjsi4zsz3qarxhl2y5ngzzffiqdostq";
            Organisation = "https://dev.azure.com/nkdagility-preview/";
            Project = "NeedToSetThis";
            ReflectedWorkItemIdField = "Custom.ReflectedWorkItemId";
        }
    }

    public interface IAzureDevOpsEndpointOptions
    {
        public string AccessToken { get; }
        public string Organisation { get; }
        public string Project { get; }
        public string ReflectedWorkItemIdField { get; }
        public AuthenticationMode AuthenticationMode { get; }
    }
}