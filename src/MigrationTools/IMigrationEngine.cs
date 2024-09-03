using System;
using MigrationTools.Clients;
using MigrationTools._EngineV1.Containers;
using MigrationTools.Endpoints;
using MigrationTools.Processors;
using MigrationTools.Processors.Infrastructure;

namespace MigrationTools
{
    public interface IMigrationEngine
    {
        ProcessingStatus Run();

    }
}