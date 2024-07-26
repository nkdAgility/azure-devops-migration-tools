using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MigrationTools.Services;

namespace MigrationTools.Tests
{
    class FakeMigrationToolVersionInfo : MigrationToolVersionInfo
    {
        public FakeMigrationToolVersionInfo(string productVersion, string fileVersion, string gitTag)
        {
            ProductVersion = productVersion;
            FileVersion = fileVersion;
            GitTag = gitTag;
        }
    }
}
