using MigrationTools._EngineV1.Clients;
using MigrationTools._EngineV1.Containers;
using MigrationTools.Processors;

namespace MigrationTools
{
    public interface IMigrationEngine
    {
        ProcessingStatus Run();

        IMigrationClient Source { get; }

        IMigrationClient Target { get; }

        TypeDefinitionMapContainer TypeDefinitionMaps { get; }
        GitRepoMapContainer GitRepoMaps { get; }
        ChangeSetMappingContainer ChangeSetMapps { get; }
        FieldMapContainer FieldMaps { get; }
    }
}