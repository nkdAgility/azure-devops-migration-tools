using System.Collections.Generic;
using MigrationTools.Endpoints;
using MigrationTools.Endpoints.Infrastructure;
using MigrationTools.Enrichers;

namespace MigrationTools._EngineV1.Configuration
{
    public class EngineConfiguration
    {
        public EngineConfiguration()
        {
            LogLevel = "Information";
        }
        public string LogLevel { get; private set; }

    }
}
