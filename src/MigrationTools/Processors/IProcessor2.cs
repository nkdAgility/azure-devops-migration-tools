using MigrationTools.EndPoints;
using MigrationTools.Enrichers;

namespace MigrationTools.Processors
{
    public interface IProcessor : MigrationTools._EngineV1.Containers.IProcessor
    {
        EndpointContainer Endpoints { get; }
        ProcessorEnricherContainer ProcessorEnrichers { get; }
    }
}