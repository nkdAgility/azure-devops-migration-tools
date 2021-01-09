namespace MigrationTools._EngineV1.Configuration
{
    public interface IEngineConfigurationBuilder
    {
        EngineConfiguration BuildDefault();

        EngineConfiguration BuildDefault2();

        EngineConfiguration BuildWorkItemMigration();

        EngineConfiguration BuildWorkItemMigration2();

        EngineConfiguration CreateEmptyConfig();
    }

    public interface IEngineConfigurationReader
    {
        EngineConfiguration BuildFromFile(string configFile = "configuration.json");
    }

    public interface ISettingsWriter
    {
        void WriteSettings(EngineConfiguration engineConfiguration, string settingsFileName);
    }
}