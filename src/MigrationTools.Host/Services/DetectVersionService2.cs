using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Logging;
using MigrationTools.DataContracts.Pipelines;
using MigrationTools.EndpointEnrichers;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using Serilog;
using WGetNET;
using Version = System.Version;

namespace MigrationTools.Host.Services
{
    public class DetectVersionService2 : IDetectVersionService2
    {
        private readonly ITelemetryLogger _Telemetry;
        private ILogger<IDetectVersionService2> _logger;

        public string PackageId { get; set; }

        private WinGetPackageManager _packageManager;
        private WinGetPackage _package = null;
        private bool _packageChecked = false;
        private bool _ServiceInitialised = false;
        private Guid UniqueID = Guid.NewGuid();

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
                return GetRunningVersion().version;
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

        public bool IsPackageManagerInstalled {
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
                return !string.IsNullOrEmpty( GetRunningVersion().PreReleaseLabel);
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
                return GetRunningVersion().PreReleaseLabel.ToLower() == "local";
            }
        }

        public bool IsNewLocalVersionAvailable
        {
            get
            {
                return (IsRunningInDebug) ? false : (IsPackageInstalled) ? !(RunningVersion >= InstalledVersion) : false;
            }
        }

        public DetectVersionService2(ITelemetryLogger telemetry, ILogger<IDetectVersionService2> logger)
        {
            _Telemetry = telemetry;
            _logger = logger;
            if (IsPreviewVersion)
            {
                PackageId = "nkdAgility.AzureDevOpsMigrationTools.Preview";
            } else
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

        public static (Version version, string PreReleaseLabel, string versionString) GetRunningVersion()
        {
            FileVersionInfo myFileVersionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly()?.Location);
            var matches = Regex.Matches(myFileVersionInfo.ProductVersion, @"^(?<major>0|[1-9]\d*)\.(?<minor>0|[1-9]\d*)\.(?<build>0|[1-9]\d*)(?:-((?<label>:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*)(?:\.(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*))*))?(?:\+(?<fullEnd>[0-9a-zA-Z-]+(?:\.[0-9a-zA-Z-]+)*))?$");
            Version version = new Version(myFileVersionInfo.FileVersion);
            string textVersion = "v" + version.Major + "." + version.Minor + "." + version.Build + "-" + matches[0].Groups[1].Value;
            return (version, matches[0].Groups[1].Value, textVersion);
        }
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