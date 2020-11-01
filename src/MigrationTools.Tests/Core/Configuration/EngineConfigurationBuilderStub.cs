using Microsoft.Extensions.Logging;

namespace MigrationTools.Configuration.Tests
{
    public class EngineConfigurationBuilderStub : IEngineConfigurationBuilder
    {
        private IEngineConfigurationBuilder ecb;

        public EngineConfigurationBuilderStub(ILogger<EngineConfigurationBuilder> logger)
        {
            ecb = new EngineConfigurationBuilder(logger);
        }

        public EngineConfiguration BuildDefault()
        {
            return ecb.BuildDefault();
        }

        public EngineConfiguration BuildDefault2()
        {
            return ecb.BuildDefault2();
        }

        public EngineConfiguration BuildFromFile(string configFile = "configuration.json")
        {
            return ecb.BuildDefault();
        }

        public EngineConfiguration BuildWorkItemMigration()
        {
            return ecb.BuildWorkItemMigration();
        }

        public EngineConfiguration BuildWorkItemMigration2()
        {
            return ecb.BuildWorkItemMigration2();
        }

        public EngineConfiguration CreateEmptyConfig()
        {
            return ecb.CreateEmptyConfig();
        }
    }
}