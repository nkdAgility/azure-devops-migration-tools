using MigrationTools.Configuration;

namespace _VstsSyncMigrator.Engine.Tests
{
    public class FakeMigrationClientConfig : IMigrationClientConfig
    {
        public IMigrationClientConfig PopulateWithDefault()
        {
            return this;
        }

        public override string ToString()
        {
            return "FakeMigration";
        }
    }
}