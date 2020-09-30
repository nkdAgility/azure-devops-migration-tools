using System;
using System.Collections.Generic;
using System.Text;
using MigrationTools.Core.Clients;
using MigrationTools.Core.Engine.Containers;

namespace MigrationTools.Core
{
    public interface IMigrationEngine
    {
        ProcessingStatus Run();
        IMigrationClient Source { get;  }

        IMigrationClient Target { get;  }

        ProcessorContainer Processors { get; }
        TypeDefinitionMapContainer TypeDefinitionMaps { get; }
        GitRepoMapContainer GitRepoMaps { get; }
        ChangeSetMappingContainer ChangeSetMapps { get; }
        FieldMapContainer FieldMaps { get; }
    }
}
