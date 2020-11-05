using MigrationTools._EngineV1.Containers;
using MigrationTools.Clients;
using MigrationTools.Configuration;
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