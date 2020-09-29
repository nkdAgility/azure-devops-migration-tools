using Microsoft.Extensions.Logging;

namespace MigrationTools.Core.Configuration.Tests
{
    public class EngineConfigurationBuilderStub : IEngineConfigurationBuilder
    {
        IEngineConfigurationBuilder ecb;

        public EngineConfigurationBuilderStub(ILogger<EngineConfigurationBuilder> logger)
        {
            ecb = new EngineConfigurationBuilder(logger);
        }

        public EngineConfiguration BuildDefault()
        {
           return  ecb.BuildDefault();
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