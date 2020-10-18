using System.Collections.ObjectModel;
using MigrationTools.Endpoints;
using MigrationTools.Engine.Containers;
using MigrationTools.Enrichers;

namespace MigrationTools.Processors
{
    public interface IProcessor2 : IProcessor
    {
        Collection<IEndpoint> Endpoints { get; }
        Collection<IProcessorEnricher> Enrichers { get; }
    }
}