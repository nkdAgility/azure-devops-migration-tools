using MigrationTools.Endpoints;
using MigrationTools.Enrichers;

namespace MigrationTools.Processors
{
    public interface IProcessor : MigrationTools._EngineV1.Containers.IProcessor
    {
        EndpointContainer Endpoints { get; }
        bool SupportsProcessorEnrichers { get; }
        ProcessorEnricherContainer ProcessorEnrichers { get; }
    }
}