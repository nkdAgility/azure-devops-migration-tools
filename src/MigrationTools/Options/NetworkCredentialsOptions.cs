using System;

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
        public string Password { get; set; }
    }
}
