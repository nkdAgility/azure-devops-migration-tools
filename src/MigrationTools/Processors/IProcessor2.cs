using MigrationTools._Enginev1.Containers;
using MigrationTools.EndPoints;
using MigrationTools.Enrichers;

namespace MigrationTools.Processors
{
    public interface IProcessor2 : IProcessor
    {
        EndpointContainer Endpoints { get; }
        ProcessorEnricherContainer ProcessorEnrichers { get; }
    }
}