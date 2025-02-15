﻿using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using MigrationTools.Services;
using Serilog;
using WGetNET;
using Version = System.Version;

namespace MigrationTools.Host.Services
{
    public class DetectVersionService2 : IDetectVersionService2
    {
        private IMigrationToolVersion _VerionsInfo;
        private readonly ITelemetryLogger _Telemetry;
        private ILogger<IDetectVersionService2> _logger;

        public string PackageId { get; set; }

        private WinGetPackageManager _packageManager;
        private WinGetPackage _package = null;
        private bool _packageChecked = false;

        private WinGetPackage Package
        {
            get
            {
                return GetPackage();
            }
        }

        public Version RunningVersion
        {
            get
            {
                return _VerionsInfo.GetRunningVersion().version;
            }
        }
        public Version AvailableVersion
        {
            get
            {
                return GetAvailableVersion();
            }
        }

        private Version GetAvailableVersion()
        {
            if (Package != null)
            {
                return Package.AvailableVersion;
            }
            return new Version("0.0.0");
        }

        public Version InstalledVersion
        {
            get
            {
                return GetInstalledVersion();
            }
        }

        private Version GetInstalledVersion()
        {
            if (Package != null)
            {
                return Package.Version;
            }
            return new Version("0.0.0");
        }

        public bool IsPackageInstalled
        {
            get
            {
                return GetIsPackageInstalled();
            }
        }

        private bool GetIsPackageInstalled()
        {
            return Package != null;
        }

        public bool IsPackageManagerInstalled
        {
            get
            {
                return GetIsPackageManagerInstalled();
            }
        }

        private bool GetIsPackageManagerInstalled()
        {
            WinGet winget = new WinGet();
            return winget.IsInstalled;
        }

        public bool IsPreviewVersion
        {
            get
            {
                return !string.IsNullOrEmpty(_VerionsInfo.GetRunningVersion().PreReleaseLabel);
            }
        }

        public bool IsUpdateAvailable
        {
            get
            {
                return (IsPackageInstalled) ? (InstalledVersion < AvailableVersion) : false;
            }
        }

        public bool IsRunningInDebug
        {
            get
            {
                return _VerionsInfo.GetRunningVersion().PreReleaseLabel.ToLower() == "local";
            }
        }

        public bool IsNewLocalVersionAvailable
        {
            get
            {
                return (IsRunningInDebug) ? false : (IsPackageInstalled) ? !(RunningVersion >= InstalledVersion) : false;
            }
        }

        public DetectVersionService2(ITelemetryLogger telemetry, ILogger<IDetectVersionService2> logger, IMigrationToolVersion verionsInfo)
        {
            _VerionsInfo = verionsInfo;
            _Telemetry = telemetry;
            _logger = logger;
            if (IsPreviewVersion)
            {
                PackageId = "nkdAgility.AzureDevOpsMigrationTools.Preview";
            }
            else
            {
                PackageId = "nkdAgility.AzureDevOpsMigrationTools";
            }

        }

        private WinGetPackage GetPackage()
        {
            if (!_packageChecked)
            {
                if (IsPackageManagerInstalled && _package == null)
                {
                    _packageManager = new WinGetPackageManager();
                    Log.Debug("Searching for package!");
                    _package = _packageManager.GetInstalledPackages(PackageId).Find(p => p.Id == PackageId);
                    Log.Debug("Found package with id {PackageId}", PackageId);
                }
                _packageChecked = true;
            }
            return _package;
        }

        public class Benchmark : IDisposable
        {
            private readonly Stopwatch timer = new Stopwatch();
            private readonly string benchmarkName;

            public TimeSpan Elapsed
            {
                get
                {
                    return timer.Elapsed;
                }
            }

            public Benchmark(string benchmarkName)
            {
                this.benchmarkName = benchmarkName;
                timer.Start();
                Log.Verbose("{benchmarkName}||START", benchmarkName);
            }

            public void Dispose()
            {
                timer.Stop();
                Log.Verbose("{benchmarkName}||STOP Elapsed: {timerElapsed}", benchmarkName, timer.Elapsed);
            }
        }

    }
}
