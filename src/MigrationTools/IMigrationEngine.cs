using MigrationTools._EngineV1.Clients;
using MigrationTools._EngineV1.Configuration;
using MigrationTools._EngineV1.Containers;
using MigrationTools.Processors;

namespace MigrationTools
{
    public interface IMigrationEngine
    {
        EngineConfiguration Config { get; }

        ProcessingStatus Run();

        IMigrationClient Source { get; }

        IMigrationClient Target { get; }

        ProcessorContainer Processors { get; }
        TypeDefinitionMapContainer TypeDefinitionMaps { get; }
        GitRepoMapContainer GitRepoMaps { get; }
        ChangeSetMappingContainer ChangeSetMapps { get; }
        FieldMapContainer FieldMaps { get; }
    }
}