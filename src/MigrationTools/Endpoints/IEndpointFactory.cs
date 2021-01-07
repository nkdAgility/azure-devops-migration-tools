namespace MigrationTools.Endpoints
{
    public interface IEndpointFactory
    {
        IEndpoint CreateEndpoint(string name);
    }
}
