namespace MigrationTools.Endpoints
{
    public interface IEndpoint
    {
        int Count { get; }

        void Configure(IEndpointOptions options);
    }
}