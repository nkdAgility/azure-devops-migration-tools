using System;
using System.Collections.Generic;
using System.Text;
using MigrationTools.Services;

namespace MigrationTools.TestExtensions
{
    public class MigrationToolVersionFake : IMigrationToolVersion
    {
        public (Version version, string PreReleaseLabel, string versionString) GetRunningVersion()
        {
            return (new Version("0.0.0"), "Test", "0.0.0-Test");
        }
    }
}
