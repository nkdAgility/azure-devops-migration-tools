namespace MigrationTools._EngineV1.Configuration
{
    public interface IMigrationClientConfig
    {
        IMigrationClientConfig PopulateWithDefault();

        string ToString();
    }
}