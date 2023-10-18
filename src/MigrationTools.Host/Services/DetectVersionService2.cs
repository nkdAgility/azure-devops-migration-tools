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


        public DetectVersionService2(ITelemetryLogger telemetry)
        {
            _Telemetry = telemetry;
            PackageId = "nkdAgility.AzureDevOpsMigrationTools";

            RunningVersion = Assembly.GetEntryAssembly().GetName().Version;
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
                    packageManager = new WinGetPackageManager();
                }
                else
                {
                    Log.Error("The Windows Package Manager is not installed, we use it to determine if you have the latest version, and to make sure that this applicaiton is up to date. You can download and install it from https://aka.ms/getwinget. After which you can call `winget install {PackageId}` from the Windows Terminal to get a manged version of this program.", PackageId);
                }
                try
                {

                    if (IsPackageManagerInstalled)
                    {
                        packageManager.GetInstalledPackages(PackageId);
                        package = packageManager.GetInstalledPackages(PackageId).GroupBy(e => e.Id, (id, g) => g.First()).SingleOrDefault();
                        //RunningVersion = version;
                        AvailableVersion = new Version(package.AvailableVersion);
                        InstalledVersion = new Version(package.Version);
                        if (package != null)
                        {
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

        public bool IsRunningInDebug()
        {
            return RunningVersion == new Version("0.0.0.1");
        }

        public bool IsUpdateAvailable()
        {
            return !(InstalledVersion >= AvailableVersion);
        }

        public bool IsNewLocalVersionAvailable()
        {
            return !(RunningVersion >= InstalledVersion);
        }

        public void UpdateFromSource()
        {
            using (var bench = new Benchmark("DetectVersionService2::UpdateFromSource"))
            {
                if (IsPackageInstalled) && IsUpdateAvailable())
                {
                    Log.Information("Running winget update {PackageId} from v{InstalledVersion} to v{AvailableVersion}", PackageId, InstalledVersion, AvailableVersion);
                    System.Threading.Tasks.Task t = packageManager.UpgradePackageAsync(PackageId);
                    while (!t.IsCompleted)
                    {
                        Log.Information("Update running...");
                        System.Threading.Thread.Sleep(3000);
                    }
                    Log.Information("Update Complete...");

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
                }
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
            Log.Debug("{benchmarkName}||START", benchmarkName);
        }

        public void Dispose()
        {
            timer.Stop();
            Log.Debug("{benchmarkName}||STOP Elapsed: {timerElapsed}", benchmarkName, timer.Elapsed);
        }
    }

}