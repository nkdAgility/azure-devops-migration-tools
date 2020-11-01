namespace MigrationTools.Endpoints
{
    public class FileSystemWorkItemEndpointOptions : EndpointOptions
    {
        public override string Endpoint => nameof(FileSystemWorkItemEndpoint);

        public override void SetDefaults()
        {
            Direction = EndpointDirection.NotSet;
        }
    }
}