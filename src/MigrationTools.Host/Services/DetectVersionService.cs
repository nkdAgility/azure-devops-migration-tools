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
using MigrationTools.CommandLine;
using MigrationTools.CommandLine.Interfaces;
using Serilog;

namespace MigrationTools.Host.Services
{
    public class DetectVersionService : IDetectVersionService
    {
        private readonly ITelemetryLogger _Telemetry;

        private readonly CommonOptions _commonOptions;

        public DetectVersionService(ITelemetryLogger telemetry, CommonOptions commonOptions)
        {
            _Telemetry = telemetry;
            _commonOptions = commonOptions;
        }
        public Version GetLatestVersion()
        {
            DateTime startTime = DateTime.Now;
            Stopwatch mainTimer = Stopwatch.StartNew();
            //////////////////////////////////
            Version latestPackageVersion = null;
            string packageID = "vsts-sync-migrator";
            bool sucess = false;
            try
            {
                //Connect to the official package repository
                IEnumerable<NuGetVersion> versions = GetVersions(packageID);
                latestPackageVersion = versions.Max(p => p.Version);
                if (latestPackageVersion != null)
                {
                    sucess = true;
                }
                _Telemetry.TrackDependency(new DependencyTelemetry("PackageRepository", "chocolatey.org", "vsts-sync-migrator", latestPackageVersion == null ? "nullVersion" : latestPackageVersion.ToString(), startTime, mainTimer.Elapsed, "200", sucess));
            }
            catch (Exception ex)
            {
                Log.Error(ex, "DetectVersionService");
                sucess = false;
                _Telemetry.TrackDependency(new DependencyTelemetry("PackageRepository", "chocolatey.org", "vsts-sync-migrator", latestPackageVersion == null ? "nullVersion" : latestPackageVersion.ToString(), startTime, mainTimer.Elapsed, "500", sucess));
            }
            /////////////////
            mainTimer.Stop();
            return latestPackageVersion;
        }

        public bool SkipUpdateCheck()
        {
            return _commonOptions.SkipUpdateCheck;
        }

        private IEnumerable<NuGetVersion> GetVersions(string packageId, string sourceUrl = "https://chocolatey.org/api/v2/")
        {
            NuGet.Common.ILogger logger = NullLogger.Instance;
            CancellationToken cancellationToken = CancellationToken.None;
            SourceCacheContext cache = new SourceCacheContext();
            PackageSource ps = new PackageSource(sourceUrl);
            var sourceRepository = Repository.Factory.GetCoreV2(ps);
            FindPackageByIdResource resource = sourceRepository.GetResourceAsync<FindPackageByIdResource>().GetAwaiter().GetResult();
            IEnumerable<NuGetVersion> versions = resource.GetAllVersionsAsync(packageId, cache, logger, cancellationToken).GetAwaiter().GetResult();
            return versions;
        }
    }
}