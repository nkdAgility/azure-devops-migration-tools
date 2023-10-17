using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.ApplicationInsights.DataContracts;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using Serilog;
using WGetNET;

namespace MigrationTools.Host.Services
{
    public class DetectVersionService : IDetectVersionService
    {
        private readonly ITelemetryLogger _Telemetry;

        public DetectVersionService(ITelemetryLogger telemetry)
        {
            _Telemetry = telemetry;
        }

        public Version GetLatestVersion()
        {
            DateTime startTime = DateTime.Now;
            Stopwatch mainTimer = Stopwatch.StartNew();
            //////////////////////////////////
            Version latestPackageVersion = null;
            string packageID = "nkdAgility.AzureDevOpsMigrationTools";
            bool sucess = false;
            try
            {
                WinGetPackageManager packageManager = new WinGetPackageManager();
                var package = packageManager.SearchPackage(packageID).SingleOrDefault();

                latestPackageVersion = new Version(package.AvailableVersion);
                if (latestPackageVersion != null)
                {
                    sucess = true;
                }
                _Telemetry.TrackDependency(new DependencyTelemetry("PackageRepository", "winget", packageID, latestPackageVersion == null ? "nullVersion" : latestPackageVersion.ToString(), startTime, mainTimer.Elapsed, "200", sucess));
            }
            catch (Exception ex)
            {
                Log.Error(ex, "DetectVersionService");
                sucess = false;
                _Telemetry.TrackDependency(new DependencyTelemetry("PackageRepository", "winget", packageID, latestPackageVersion == null ? "nullVersion" : latestPackageVersion.ToString(), startTime, mainTimer.Elapsed, "500", sucess));
            }
            /////////////////
            mainTimer.Stop();
            return latestPackageVersion;
        }

    }
}