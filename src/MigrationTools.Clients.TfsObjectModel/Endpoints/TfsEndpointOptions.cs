using System.ComponentModel.DataAnnotations;
using MigrationTools.Endpoints.Infrastructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace MigrationTools.Endpoints
{
    public class TfsEndpointOptions : EndpointOptions
    {
        [JsonConverter(typeof(StringEnumConverter))]
        [Required]
        public AuthenticationMode AuthenticationMode { get; set; }

        [Required]
        public string AccessToken { get; set; }
        [JsonProperty(Order = -3)]
        [Required]
        public string Organisation { get; set; }
        [JsonProperty(Order = -2)]
        [Required]
        public string Project { get; set; }

        [JsonProperty(Order = -1)]
        [Required]
        public string ReflectedWorkItemIdField { get; set; }

        [Required]
        public TfsLanguageMapOptions LanguageMaps { get; set; }


        //right now this method is reflection invoked to generate the first settings file
        private void SetDefaults()
        {
            AccessToken = "6i4jyylsadkjanjniaydxnjsi4zsz3qarxhl2y5ngzzffiqdostq";
            Organisation = "https://dev.azure.com/nkdagility-preview/";
            Project = "NeedToSetThis";
            ReflectedWorkItemIdField = "Custom.ReflectedWorkItemId";
            LanguageMaps = new TfsLanguageMapOptions()
            {
                AreaPath = "Area",
                IterationPath = "Iteration"
            };
        }
    }
}