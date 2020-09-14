using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NuGet;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;

namespace MigrationTools.Services
{
    public class DetectVersionService : IDetectVersionService
    {
        private readonly TelemetryClient _Telemetry;


        public DetectVersionService(TelemetryClient telemetry)
        {
            _Telemetry = telemetry;
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
            }
            catch (Exception ex)
            {
                _Telemetry.TrackException(ex);
                sucess = false;
            }
            /////////////////
            mainTimer.Stop();
            _Telemetry.TrackDependency(new DependencyTelemetry("PackageRepository", "chocolatey.org", "vsts-sync-migrator", latestPackageVersion == null ? "nullVersion" : latestPackageVersion.ToString(), startTime, mainTimer.Elapsed, null, sucess));
            return latestPackageVersion;
        }

        private  IEnumerable<NuGetVersion> GetVersions(string packageId, string sourceUrl = "https://chocolatey.org/api/v2/")
        {
            NuGet.Common.ILogger logger = NullLogger.Instance;
            CancellationToken cancellationToken = CancellationToken.None;
            SourceCacheContext cache = new SourceCacheContext();
            PackageSource ps = new PackageSource(sourceUrl);
            var sourceRepository = Repository.Factory.GetCoreV2(ps);
            FindPackageByIdResource resource = sourceRepository.GetResourceAsync<FindPackageByIdResource>().GetAwaiter().GetResult();
            IEnumerable<NuGetVersion> versions =  resource.GetAllVersionsAsync(packageId, cache, logger, cancellationToken).GetAwaiter().GetResult();
            return versions;
        }
    }
}