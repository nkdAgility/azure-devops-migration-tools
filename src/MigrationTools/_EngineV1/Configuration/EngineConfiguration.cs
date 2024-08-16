using System.Collections.Generic;
using MigrationTools.Enrichers;

namespace MigrationTools._EngineV1.Configuration
{
    public class EngineConfiguration
    {
        public EngineConfiguration()
        {
            LogLevel = "Information";
        }

        public IMigrationClientConfig Source { get; set; }
        public IMigrationClientConfig Target { get; set; }
        public string LogLevel { get; private set; }
        public string Version { get; set; }



    }
}
