using System;
using MigrationTools._EngineV1.Clients;
using MigrationTools._EngineV1.Containers;
using MigrationTools.Processors;
using MigrationTools.Processors.Infrastructure;

namespace MigrationTools
{
    public interface IMigrationEngine
    {
        ProcessingStatus Run();

        IMigrationClient Source { get; }

        IMigrationClient Target { get; }
    }
}