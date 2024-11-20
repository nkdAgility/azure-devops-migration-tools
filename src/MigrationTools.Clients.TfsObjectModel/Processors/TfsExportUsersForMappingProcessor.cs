using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MigrationTools.Clients;
using MigrationTools.DataContracts;
using MigrationTools.Enrichers;
using MigrationTools.Processors.Infrastructure;
using MigrationTools.Tools;
using Newtonsoft.Json;

namespace MigrationTools.Processors
{
    /// <summary>
    /// ExportUsersForMappingContext is a tool used to create a starter mapping file for users between the source and target systems.
    /// Use `ExportUsersForMappingConfig` to configure.
    /// </summary>
    /// <status>ready</status>
    /// <processingtarget>Work Items</processingtarget>
    public class TfsExportUsersForMappingProcessor : TfsProcessor
    {
        public TfsExportUsersForMappingProcessor(
            IOptions<TfsExportUsersForMappingProcessorOptions> options,
            TfsCommonTools tfsCommonTools,
            ProcessorEnricherContainer processorEnrichers,
            IServiceProvider services,
            ITelemetryLogger telemetry,
            ILogger<TfsExportUsersForMappingProcessor> logger)
            : base(options, tfsCommonTools, processorEnrichers, services, telemetry, logger)
        {
        }

        new TfsExportUsersForMappingProcessorOptions Options => (TfsExportUsersForMappingProcessorOptions)base.Options;

        new TfsTeamProjectEndpoint Source => (TfsTeamProjectEndpoint)base.Source;

        new TfsTeamProjectEndpoint Target => (TfsTeamProjectEndpoint)base.Target;

        protected override void InternalExecute()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            CheckOptions();

            IdentityMapResult data;
            if (Options.OnlyListUsersInWorkItems)
            {
                Log.LogInformation("OnlyListUsersInWorkItems is true, only users in work items will be listed");
                List<WorkItemData> sourceWorkItems = Source.WorkItems.GetWorkItems(Options.WIQLQuery);
                Log.LogInformation("Processed {0} work items from Source", sourceWorkItems.Count);

                data = CommonTools.UserMapping.GetUsersInSourceMappedToTargetForWorkItems(this, sourceWorkItems);
                Log.LogInformation("Found {usersToMap} total mapped", data.IdentityMap.Count);
            }
            else
            {
                Log.LogInformation("OnlyListUsersInWorkItems is false, all users will be listed");
                data = CommonTools.UserMapping.GetUsersInSourceMappedToTarget(this);
                Log.LogInformation("Found {usersToMap} total mapped", data.IdentityMap.Count);
            }

            List<IdentityMapData> usersToMap = data.IdentityMap.Where(x => x.Source.DisplayName != x.Target?.DisplayName).ToList();
            Log.LogInformation("Filtered to {usersToMap} total viable mappings", usersToMap.Count);
            Dictionary<string, string> usermappings = [];
            foreach (IdentityMapData userMapping in usersToMap)
            {
                // We cannot use ToDictionary(), because there can be multiple users with the same display name and so
                // it would throw with duplicate key. This way we just overwrite the value – last item in source wins.
                usermappings[userMapping.Source.DisplayName] = userMapping.Target?.DisplayName;
            }
            File.WriteAllText(CommonTools.UserMapping.Options.UserMappingFile, JsonConvert.SerializeObject(usermappings, Formatting.Indented));
            Log.LogInformation("User mappings writen to: {LocalExportJsonFile}", CommonTools.UserMapping.Options.UserMappingFile);
            if (Options.ExportAllUsers)
            {
                ExportAllUsers(data);
            }

            stopwatch.Stop();
            Log.LogInformation("DONE in {Elapsed} seconds", stopwatch.Elapsed);
        }

        private void ExportAllUsers(IdentityMapResult data)
        {
            var allUsers = new
            {
                data.SourceUsers,
                data.TargetUsers
            };
            File.WriteAllText(Options.UserExportFile, JsonConvert.SerializeObject(allUsers, Formatting.Indented));
            Log.LogInformation("All user writen to: {exportFile}", Options.UserExportFile);
        }

        private void CheckOptions()
        {
            if (string.IsNullOrEmpty(CommonTools.UserMapping.Options.UserMappingFile))
            {
                Log.LogError("UserMappingFile is not set");
                throw new ArgumentNullException("UserMappingFile must be set on the TfsUserMappingToolOptions in CommonTools.");
            }
            if (Options.ExportAllUsers && string.IsNullOrEmpty(Options.UserExportFile))
            {
                Log.LogError($"Flag ExportAllUsers is set but export file UserExportFile is not set.");
                throw new ArgumentNullException("UserExportFile must be set on the TfsExportUsersForMappingProcessorOptions in Processors.");
            }
        }
    }
}
