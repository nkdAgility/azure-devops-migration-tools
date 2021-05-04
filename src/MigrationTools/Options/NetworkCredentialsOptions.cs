namespace MigrationTools.Options
{
    public class NetworkCredentialsOptions
    {
        public Credentials Source { get; set; }
        public Credentials Target { get; set; }
    }

    public class Credentials
    {
        public string Domain { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}
