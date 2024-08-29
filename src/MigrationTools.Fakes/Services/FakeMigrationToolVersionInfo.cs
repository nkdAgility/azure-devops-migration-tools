using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MigrationTools.Services;

namespace MigrationTools.Services.Shadows
{
    public class FakeMigrationToolVersionInfo : MigrationToolVersionInfo
    {
        public FakeMigrationToolVersionInfo()
        {
            ProductVersion = "0.0.0-test+7acec2e6266f5f05b2807264ee8f1db7b94b1949";
            FileVersion = "0.0.0.0";
            GitTag = "v0.0.0-test.0-0-g7acec2e";
        }
        public FakeMigrationToolVersionInfo(string productVersion, string fileVersion, string gitTag)
        {
            ProductVersion = productVersion;
            FileVersion = fileVersion;
            GitTag = gitTag;
        }
    }
}
