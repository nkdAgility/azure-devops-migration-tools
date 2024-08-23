using System.Collections.Generic;
using MigrationTools.Endpoints;
using MigrationTools.Enrichers;

namespace MigrationTools._EngineV1.Configuration
{
    public class EngineConfiguration
    {
        public EngineConfiguration()
        {
            LogLevel = "Information";
        }

        public IEndpointOptions Source { get; set; }
        public IEndpointOptions Target { get; set; }
        public string LogLevel { get; private set; }

    }
}
