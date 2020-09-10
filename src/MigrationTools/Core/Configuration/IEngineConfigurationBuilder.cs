using System;
using System.Collections.Generic;
using System.Text;

namespace MigrationTools.Core.Configuration
{
    public interface IEngineConfigurationBuilder
    {
        EngineConfiguration BuildFromFile();
        EngineConfiguration BuildDefault();
        EngineConfiguration BuildWorkItemMigration();

    }
}
