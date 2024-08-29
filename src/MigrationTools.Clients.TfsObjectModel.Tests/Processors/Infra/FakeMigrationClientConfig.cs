using System;
using MigrationTools._EngineV1.Configuration;
using MigrationTools.Endpoints;
using MigrationTools.Endpoints.Infrastructure;

namespace _VstsSyncMigrator.Engine.Tests
{
    [Obsolete]
    public class FakeMigrationClientConfig : EndpointOptions
    {
        public IEndpointOptions PopulateWithDefault()
        {
            return this;
        }

        public override string ToString()
        {
            return "FakeMigration";
        }
    }
}