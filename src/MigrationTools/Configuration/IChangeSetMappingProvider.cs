using System.Collections.Generic;

namespace MigrationTools.Configuration
{
    public interface IChangeSetMappingProvider
    {
        void ImportMappings(Dictionary<int, string> changesetMappingStore);
    }
}
