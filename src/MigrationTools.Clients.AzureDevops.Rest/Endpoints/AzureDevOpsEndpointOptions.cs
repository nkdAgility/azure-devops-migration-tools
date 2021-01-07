using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MigrationTools.Endpoints
{
    public class AzureDevOpsEndpointOptions : EndpointOptions
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


        //public override void SetDefaults()
        //{
        //    base.SetDefaults();
        //    AccessToken = MigrationTools.Tests.TestingConstants.AccessToken;
        //    Organisation = "https://dev.azure.com/nkdagility-preview/";
        //    Project = "NeedToSetThis";
        //    ReflectedWorkItemIdField = "Custom.ReflectedWorkItemId";
        //}
    }
}