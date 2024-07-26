using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MigrationTools.Host.Services;

namespace MigrationTools.Host.Tests.Services
{
    class FakeMigrationToolVersionInfo : IMigrationToolVersionInfo
    {
        public string ProductVersion { get; private set; }
        public string FileVersion { get; private set; }
        public string GitTag { get; private set; }

        public FakeMigrationToolVersionInfo(string productVersion, string fileVersion, string gitTag)
        {
            ProductVersion = productVersion;
            FileVersion = fileVersion;
            GitTag = gitTag;
        }
    }
}
