using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.ApplicationInsights.DataContracts;
using MigrationTools.DataContracts.Pipelines;
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
        public Version RunningVersion { get; private set; }
        public Version AvailableVersion { get; private set; }

        public Version InstalledVersion { get; private set; }

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
            Stopwatch mainTimer = Stopwatch.StartNew();
            //////////////////////////////////
            bool sucess = false;
            try
            {
                WinGetInfo wingetInfo = new WinGetInfo();
                IsPackageManagerInstalled = wingetInfo.WinGetInstalled;
                if (IsPackageManagerInstalled)
                {
                    WinGetPackageManager packageManager = new WinGetPackageManager();
                    var package = packageManager.GetInstalledPackages(PackageId).GroupBy(e => e.Id, (id, g) => g.First()).SingleOrDefault();
                    //RunningVersion = version;
                    AvailableVersion = new Version(package.AvailableVersion);
                    InstalledVersion = new Version(package.Version);
                    if (package != null)
                    {
                        sucess = true;
                    }
                    _Telemetry.TrackDependency(new DependencyTelemetry("PackageRepository", "winget", PackageId, AvailableVersion == null ? "nullVersion" : AvailableVersion.ToString(), startTime, mainTimer.Elapsed, "200", sucess));
                } 
            }
            catch (Exception ex)
            {
                Log.Error(ex, "DetectVersionService");
                sucess = false;
                _Telemetry.TrackDependency(new DependencyTelemetry("PackageRepository", "winget", PackageId, AvailableVersion == null ? "nullVersion" : AvailableVersion.ToString(), startTime, mainTimer.Elapsed, "500", sucess));
            }
            /////////////////
            mainTimer.Stop();
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
    }
}