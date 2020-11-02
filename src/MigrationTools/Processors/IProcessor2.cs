using System.Collections.Generic;
using MigrationTools.Endpoints;
using MigrationTools.Engine.Containers;
using MigrationTools.Enrichers;

namespace MigrationTools.Processors
{
    public interface IProcessor2 : IProcessor
    {
        List<IEndpoint> Endpoints { get; }
        List<IProcessorEnricher> Enrichers { get; }
    }
}