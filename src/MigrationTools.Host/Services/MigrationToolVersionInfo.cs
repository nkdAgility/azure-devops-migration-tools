using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;


/* Unmerged change from project 'MigrationTools.Host (netstandard2.0)'
Before:
using NuGet.Protocol.Core.Types;
After:
using MigrationTools;
using MigrationTools.Host;
using MigrationTools.Host;
using MigrationTools.Host.Versioning;
using NuGet.Protocol.Core.Types;
*/
using NuGet.Protocol.Core.Types;

namespace MigrationTools.Host.Services
{
    public interface IMigrationToolVersionInfo
    {
        string ProductVersion { get; }
        string FileVersion { get;}
        string GitTag { get; }
    }
    internal class MigrationToolVersionInfo : IMigrationToolVersionInfo
    {
        public string ProductVersion { get; private set; }
        public string FileVersion { get; private set; }
        public string GitTag { get; private set; }

        public MigrationToolVersionInfo()
        {
            FileVersionInfo myFileVersionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly()?.Location);
            ProductVersion = myFileVersionInfo.ProductVersion;
            FileVersion = myFileVersionInfo.FileVersion;
            GitTag = ThisAssembly.Git.Tag;
        }

    }
}
