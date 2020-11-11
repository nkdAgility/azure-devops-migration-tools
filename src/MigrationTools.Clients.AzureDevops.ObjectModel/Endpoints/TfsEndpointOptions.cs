namespace MigrationTools.Endpoints
{
    public abstract class TfsEndpointOptions : EndpointOptions
    {
        public string AccessToken { get; set; }
        public string Organisation { get; set; }
        public string Project { get; set; }
    }
}