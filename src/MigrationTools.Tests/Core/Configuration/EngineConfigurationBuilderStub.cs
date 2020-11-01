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
            throw new System.NotImplementedException();
        }

        public EngineConfiguration BuildFromFile(string configFile = "configuration.json")
        {
            return ecb.BuildDefault();
        }

        public EngineConfiguration BuildWorkItemMigration()
        {
            return ecb.BuildWorkItemMigration();
        }

        public EngineConfiguration CreateEmptyConfig()
        {
            return ecb.CreateEmptyConfig();
        }
    }
}