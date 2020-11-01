namespace MigrationTools.Configuration
{
    public interface IEngineConfigurationBuilder
    {
        EngineConfiguration BuildFromFile(string configFile = "configuration.json");

        EngineConfiguration BuildDefault();

        EngineConfiguration BuildDefault2();

        EngineConfiguration BuildWorkItemMigration();

        EngineConfiguration CreateEmptyConfig();
    }
}