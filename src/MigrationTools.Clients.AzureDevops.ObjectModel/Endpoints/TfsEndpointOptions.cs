namespace MigrationTools.Endpoints
{
    public abstract class TfsEndpointOptions : EndpointOptions, ITfsEndpointOptions
    {
        public string AccessToken { get; set; }
        public string Organisation { get; set; }
        public string Project { get; set; }
    }

    public interface ITfsEndpointOptions
    {
        public string AccessToken { get; }
        public string Organisation { get; }
        public string Project { get; }
    }
}