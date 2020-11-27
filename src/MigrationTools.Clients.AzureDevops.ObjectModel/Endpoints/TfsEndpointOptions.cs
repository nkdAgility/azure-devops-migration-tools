using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MigrationTools.Endpoints
{
    public class TfsEndpointOptions : EndpointOptions, ITfsEndpointOptions
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

        public TfsLanguageMapOptions LanguageMaps { get; set; }

        public override Type ToConfigure => typeof(TfsEndpoint);

        public override void SetDefaults()
        {
            base.SetDefaults();
            AccessToken = "6i4jyylsadkjanjniaydxnjsi4zsz3qarxhl2y5ngzzffiqdostq";
            Organisation = "https://dev.azure.com/nkdagility-preview/";
            Project = "NeedToSetThis";
            ReflectedWorkItemIdField = "Custom.ReflectedWorkItemId";
            LanguageMaps = new TfsLanguageMapOptions();
            LanguageMaps.SetDefaults();
        }
    }

    public interface ITfsEndpointOptions
    {
        public string AccessToken { get; }
        public string Organisation { get; }
        public string Project { get; }
        public string ReflectedWorkItemIdField { get; }
        public AuthenticationMode AuthenticationMode { get; }
        public TfsLanguageMapOptions LanguageMaps { get; }
    }
}