using System;
using MigrationTools.Options.Infrastructure;
using Newtonsoft.Json;

namespace MigrationTools.Options
{
    [Obsolete]
    public class NetworkCredentialsOptions
    {
        public NetworkCredentials Source { get; set; }
        public NetworkCredentials Target { get; set; }
    }

    public class NetworkCredentials
    {
        public string Domain { get; set; }
        public string UserName { get; set; }

        [JsonConverter(typeof(DefaultOnlyConverter<string>), "** removed as a secret ***")]
        public string Password { get; set; }
    }
}
