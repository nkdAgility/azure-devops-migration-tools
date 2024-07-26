using MigrationTools.Services;

namespace MigrationTools.Fakes
{
    public class FakeMigrationToolVersion : IMigrationToolVersion
    {
        public (Version version, string PreReleaseLabel, string versionString) GetRunningVersion()
        {
            return (new System.Version("0.0.0"), "test", "0.0.0-test");
        }
    }
}