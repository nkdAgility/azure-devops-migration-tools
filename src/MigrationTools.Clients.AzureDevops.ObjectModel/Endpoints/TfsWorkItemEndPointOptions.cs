using System;
using MigrationTools.Options;
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

    public class TfsWorkItemEndPointOptions : EndpointOptions
    {
        public override Type ToConfigure => typeof(TfsWorkItemEndPoint);

        public QueryOptions Query { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public AuthenticationMode AuthenticationMode { get; set; }

        public string AccessToken { get; set; }
        public string Organisation { get; set; }
        public string Project { get; set; }

        public override void SetDefaults()
        {
            Direction = EndpointDirection.NotSet;
        }
    }
}