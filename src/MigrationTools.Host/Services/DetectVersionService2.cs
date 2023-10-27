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

        public string PackageId { get; set; }

        private WinGetPackageManager packageManager;
        private WinGetPackage package = null;

        public Version RunningVersion { get; private set; }
        public Version AvailableVersion { get; private set; }

        public Version InstalledVersion { get; private set; }

        public bool IsPackageInstalled { get; private set; } = false;

        public bool IsPackageManagerInstalled { get; private set; } = false;

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

        public DetectVersionService2(ITelemetryLogger telemetry)
        {
            _Telemetry = telemetry;
            PackageId = "nkdAgility.AzureDevOpsMigrationTools";
            InitialiseService();
        }

        private void InitialiseService()
        {
            DateTime startTime = DateTime.Now;
            using (var bench = new Benchmark("DetectVersionService2::InitialiseService"))
            {
                //////////////////////////////////
                WinGetInfo wingetInfo = new WinGetInfo();
                IsPackageManagerInstalled = wingetInfo.WinGetInstalled;
                if (IsPackageManagerInstalled)
                {
                    Log.Verbose("The Windows Package Manager is installed!");
                    packageManager = new WinGetPackageManager();
                }
                try
                {
                    RunningVersion = GetRunningVersion();
                    if (IsPackageManagerInstalled)
                    {
                        Log.Verbose("Searching for package!");
                        package = packageManager.GetInstalledPackages(PackageId).GroupBy(e => e.Id, (id, g) => g.First()).SingleOrDefault();
                        if (package != null)
                        {
                            AvailableVersion = new Version(package.AvailableVersion);
                            InstalledVersion = new Version(package.Version);
                            Log.Debug("Found package with id {PackageId}", PackageId);
                            IsPackageInstalled = true;
                        }
                        _Telemetry.TrackDependency(new DependencyTelemetry("PackageRepository", "winget", PackageId, AvailableVersion == null ? "nullVersion" : AvailableVersion.ToString(), startTime, bench.Elapsed, "200", IsPackageInstalled));
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "DetectVersionService");
                    IsPackageInstalled = false;
                    _Telemetry.TrackDependency(new DependencyTelemetry("PackageRepository", "winget", PackageId, AvailableVersion == null ? "nullVersion" : AvailableVersion.ToString(), startTime, bench.Elapsed, "500", IsPackageInstalled));
                }
            }
        }

        public static Version GetRunningVersion()
        {
            Version assver = new Version(14, 0,0) ;// Assembly.GetEntryAssembly()?.GetName().Version;
            if (assver == null) {
                return new Version("0.0.0");
            }
            return new Version(assver.Major, assver.Minor, assver.Build);
        }


        public void UpdateFromSource()
        {
            if (IsPackageManagerInstalled)
            {
                using (var bench = new Benchmark("DetectVersionService2::UpdateFromSource"))
                {
                    if (IsPackageInstalled && IsUpdateAvailable)
                    {
                        Log.Information("Running winget update {PackageId} from v{InstalledVersion} to v{AvailableVersion}", PackageId, InstalledVersion, AvailableVersion);
                        System.Threading.Tasks.Task t = packageManager.UpgradePackageAsync(PackageId);
                        while (!t.IsCompleted)
                        {
                            Log.Information("Update running...");
                            System.Threading.Thread.Sleep(3000);
                        }
                        Log.Information("Update Complete...");
                        InitialiseService();
                    }
                    else if (!IsPackageInstalled)
                    {
                        Log.Information("Running winget install {PackageId} from v{InstalledVersion} to v{AvailableVersion}", PackageId, InstalledVersion, AvailableVersion);

                        System.Threading.Tasks.Task t = packageManager.InstallPackageAsync(PackageId);
                        while (!t.IsCompleted)
                        {
                            Log.Information("Install running...");
                            System.Threading.Thread.Sleep(5000);
                        }
                        Log.Information("Install Complete...");
                        InitialiseService();
                    }
                }
            }
            else
            {
                Log.Information("Package Manager not installed");
            }

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