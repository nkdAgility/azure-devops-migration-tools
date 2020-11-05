using MigrationTools.EndPoints;
using MigrationTools.Engine.Containers;
using MigrationTools.Enrichers;

namespace MigrationTools.Processors
{
    public interface IProcessor2 : IProcessor
    {
        EndpointContainer Endpoints { get; }
        ProcessorEnricherContainer ProcessorEnrichers { get; }
    }
}