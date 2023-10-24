using System;

namespace MigrationTools.Host.Services
{
    public interface IDetectVersionService2
    {
        public Version AvailableVersion { get; }

        public Version InstalledVersion { get; }

public Version RunningVersion { get; }

        public bool IsPackageManagerInstalled { get; }

        public bool IsPackageInstalled { get; }
        string PackageId { get; }

        bool IsUpdateAvailable { get; }
        bool IsRunningInDebug { get; }
        bool IsNewLocalVersionAvailable { get; }

        void UpdateFromSource();
    }
}