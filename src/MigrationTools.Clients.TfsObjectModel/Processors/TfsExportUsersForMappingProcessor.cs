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

            bool detailed = CommonTools.UserMapping.Options.UseIdentityMapDataFormat;
            List<IdentityMapData> usersToMap = data.IdentityMap.Where(x => x.Source.DisplayName != x.Target?.DisplayName
                || (detailed && !string.Equals(x.Source.AccountName, x.Target?.AccountName, StringComparison.OrdinalIgnoreCase))).ToList();
            Log.LogInformation("Filtered to {usersToMap} total viable mappings", usersToMap.Count);

            if (detailed)
            {
                Log.LogInformation("Exporting user mappings in detailed IdentityMapData format");
                TfsUserMappingTool.SerializeIdentityMapData(CommonTools.UserMapping.Options.UserMappingFile, usersToMap, Log);
            }
            else
            {
                Log.LogInformation("Exporting user mappings in simple dictionary format");
                ExportSimpleDictionaryFormat(usersToMap, data.SourceUsers, data.TargetUsers);
            }

            if (Options.ExportAllUsers)
            {
                ExportAllUsers(data);
            }

            stopwatch.Stop();
            Log.LogInformation("DONE in {Elapsed} seconds", stopwatch.Elapsed);
        }

        private void ExportSimpleDictionaryFormat(List<IdentityMapData> usersToMap, List<IdentityItemData> sourceUsers, List<IdentityItemData> targetUsers)
        {
            TfsUserMappingTool.SerializeUserMap(CommonTools.UserMapping.Options.UserMappingFile,
                BuildSimpleDictionary(usersToMap, sourceUsers, targetUsers), Log);
        }

        /// <summary>
        /// Builds the simple mapping dictionary. Display names shared by more than one user are written as
        /// <c>Display Name &lt;upn&gt;</c> so each entry identifies a single user.
        /// </summary>
        public static Dictionary<string, string> BuildSimpleDictionary(List<IdentityMapData> usersToMap, List<IdentityItemData> sourceUsers, List<IdentityItemData> targetUsers)
        {
            var duplicateSource = TfsUserIdentityReference.DuplicateDisplayNames(sourceUsers);
            var duplicateTarget = TfsUserIdentityReference.DuplicateDisplayNames(targetUsers);
            Dictionary<string, string> usermappings = new(StringComparer.CurrentCultureIgnoreCase);
            foreach (IdentityMapData userMapping in usersToMap)
            {
                var key = duplicateSource.Contains(userMapping.Source.DisplayName)
                    ? TfsUserIdentityReference.Format(userMapping.Source)
                    : userMapping.Source.DisplayName;
                var target = userMapping.Target;
                usermappings[key] = target != null && duplicateTarget.Contains(target.DisplayName)
                    ? TfsUserIdentityReference.Format(target)
                    : target?.DisplayName;
            }
            return usermappings;
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
