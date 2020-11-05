using System.Collections.Generic;

namespace MigrationTools._EngineV1.Configuration
{
    public interface IChangeSetMappingProvider
    {
        void ImportMappings(Dictionary<int, string> changesetMappingStore);
    }
}