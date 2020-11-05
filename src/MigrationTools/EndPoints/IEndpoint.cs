using MigrationTools.Enrichers;

namespace MigrationTools.Endpoints
{
    public interface IEndpoint
    {
        int Count { get; }
        EndpointDirection Direction { get; }

        void Configure(IEndpointOptions options);

        EndpointEnricherContainer EndpointEnrichers { get; }
    }
}