using MigrationTools.Core.Configuration;

namespace MigrationTools.Core.Configuration.Tests
{
    public class EngineConfigurationBuilderStub : IEngineConfigurationBuilder
    {
        IEngineConfigurationBuilder ecb = new EngineConfigurationBuilder();

        public EngineConfiguration BuildDefault()
        {
           return  ecb.BuildDefault();
        }

        public EngineConfiguration BuildFromFile()
        {
            return ecb.BuildDefault();
        }

        public EngineConfiguration BuildWorkItemMigration()
        {
            return ecb.BuildWorkItemMigration();
        }
    }
}