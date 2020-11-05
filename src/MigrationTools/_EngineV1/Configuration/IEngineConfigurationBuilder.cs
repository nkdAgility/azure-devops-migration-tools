namespace MigrationTools._EngineV1.Configuration
{
    public interface IEngineConfigurationBuilder
    {
        EngineConfiguration BuildFromFile(string configFile = "configuration.json");

        EngineConfiguration BuildDefault();

        EngineConfiguration BuildDefault2();

        EngineConfiguration BuildWorkItemMigration();

        EngineConfiguration BuildWorkItemMigration2();

        EngineConfiguration CreateEmptyConfig();
    }
}