using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
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
        private bool _ServiceInitialised = false;

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
                return GetRunningVersion();
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
                return RunningVersion == new Version("0.0.0");
            }
        }

        public bool IsNewLocalVersionAvailable
        {
            get
            {
                return (IsPackageInstalled) ? !(RunningVersion >= InstalledVersion) : false;
            }
        }

        public DetectVersionService2(ITelemetryLogger telemetry, ILogger<IDetectVersionService2> logger)
        {
            _Telemetry = telemetry;
            _logger = logger;
            PackageId = "nkdAgility.AzureDevOpsMigrationTools";
        }

        private WinGetPackage GetPackage()
        {
            if (IsPackageManagerInstalled && _package == null)
            {
                _packageManager = new WinGetPackageManager();
                Log.Debug("Searching for package!");
                _package = _packageManager.GetInstalledPackages(PackageId, true).FirstOrDefault();
                Log.Debug("Found package with id {PackageId}", PackageId);
            }
            return _package;
        }

        //private void InitialiseService()
        //{
        //    _logger.LogDebug("DetectVersionService2::InitialiseService");
        //    DateTime startTime = DateTime.Now;
        //    using (var bench = new Benchmark("DetectVersionService2::InitialiseService"))
        //    {
        //        //////////////////////////////////
               
                
        //        try
        //        {
        //            if (IsPackageManagerInstalled)
        //            {
                        
        //                if (package != null)
        //                {
   
                            
        //                    IsPackageInstalled = true;
        //                }
        //                _Telemetry.TrackDependency(new DependencyTelemetry("PackageRepository", "winget", PackageId, AvailableVersion == null ? "nullVersion" : AvailableVersion.ToString(), startTime, bench.Elapsed, "200", IsPackageInstalled));
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            Log.Error(ex, "DetectVersionService");
        //            IsPackageInstalled = false;
        //            _Telemetry.TrackDependency(new DependencyTelemetry("PackageRepository", "winget", PackageId, AvailableVersion == null ? "nullVersion" : AvailableVersion.ToString(), startTime, bench.Elapsed, "500", IsPackageInstalled));
        //        }
        //    }
        //}

        public static Version GetRunningVersion()
        {
            Version assver = Assembly.GetEntryAssembly()?.GetName().Version;
            if (assver == null)
            {
                return new Version("0.0.0");
            }
            return new Version(assver.Major, assver.Minor, assver.Build);
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