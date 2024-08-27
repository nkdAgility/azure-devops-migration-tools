using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using MigrationTools.Options;
using Newtonsoft.Json.Converters;

namespace MigrationTools.Endpoints.Infrastructure
{
    public class TfsAuthenticationOptions
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public AuthenticationMode AuthenticationMode { get; set; }

        public NetworkCredentials NetworkCredentials { get; set; }
        public string AccessToken { get; set; }
    }
}
