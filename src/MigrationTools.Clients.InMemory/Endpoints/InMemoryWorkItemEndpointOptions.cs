namespace MigrationTools.Endpoints
{
    public class InMemoryWorkItemEndpointOptions : EndpointOptions
    {
        public override string Endpoint => nameof(InMemoryWorkItemEndpoint);

        public override void SetDefaults()
        {
            Direction = EndpointDirection.NotSet;
        }
    }
}