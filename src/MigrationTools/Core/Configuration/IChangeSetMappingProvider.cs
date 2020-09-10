using System;
using System.Collections.Generic;
using System.Text;

namespace MigrationTools.Core.Configuration
{
    public interface IChangeSetMappingProvider
    {
        void ImportMappings(Dictionary<int, string> changesetMappingStore);
    }
}
