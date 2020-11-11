using System.Collections.Generic;
using MigrationTools.EndpointEnrichers;

namespace MigrationTools.Endpoints
{
    public interface IEndpoint
    {
        int Count { get; }
        EndpointDirection Direction { get; }

        void Configure(IEndpointOptions options);

        EndpointEnricherContainer EndpointEnrichers { get; }
    }

    public interface ISourceEndPoint : IEndpoint
    {
        IEnumerable<IEndpointSourceEnricher> SourceEnrichers { get; }
    }

    public interface ITargetEndPoint : IEndpoint
    {
        IEnumerable<IEndpointTargetEnricher> TargetEnrichers { get; }
    }
}