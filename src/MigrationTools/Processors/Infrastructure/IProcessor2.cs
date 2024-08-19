using MigrationTools.Endpoints;
using MigrationTools.Enrichers;

namespace MigrationTools.Processors
{
    public interface IProcessor : _EngineV1.Containers.IProcessor
    {
        IEndpoint Source { get; }
        IEndpoint Target { get; }
        bool SupportsProcessorEnrichers { get; }
        ProcessorEnricherContainer ProcessorEnrichers { get; }
    }
}