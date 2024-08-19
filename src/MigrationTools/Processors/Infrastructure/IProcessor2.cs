using MigrationTools._EngineV1.Containers;
using MigrationTools.Endpoints;
using MigrationTools.Enrichers;

namespace MigrationTools.Processors.Infrastructure
{
    public interface IProcessor : IOldProcessor
    {
        IEndpoint Source { get; }
        IEndpoint Target { get; }
        bool SupportsProcessorEnrichers { get; }
        ProcessorEnricherContainer ProcessorEnrichers { get; }
    }
}