namespace MigrationTools.Configuration
{
    public interface IMigrationClientConfig
    {
        IMigrationClientConfig PopulateWithDefault();

        string ToString();
    }
}