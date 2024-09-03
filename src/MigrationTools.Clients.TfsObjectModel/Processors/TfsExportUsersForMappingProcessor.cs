using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MigrationTools;
using MigrationTools.Clients;
using MigrationTools._EngineV1.Configuration;
using MigrationTools._EngineV1.Configuration.Processing;

using MigrationTools.DataContracts;
using MigrationTools.DataContracts.Process;
using MigrationTools.EndpointEnrichers;
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
        public TfsExportUsersForMappingProcessor(IOptions<TfsExportUsersForMappingProcessorOptions> options, TfsCommonTools tfsCommonTools, ProcessorEnricherContainer processorEnrichers, IServiceProvider services, ITelemetryLogger telemetry, ILogger<TfsExportUsersForMappingProcessor> logger) : base(options, tfsCommonTools, processorEnrichers, services, telemetry, logger)
        {
        }


        new TfsExportUsersForMappingProcessorOptions Options => (TfsExportUsersForMappingProcessorOptions)base.Options;

        new TfsTeamProjectEndpoint Source => (TfsTeamProjectEndpoint)base.Source;

        new TfsTeamProjectEndpoint Target => (TfsTeamProjectEndpoint)base.Target;

        protected override void InternalExecute()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            if(string.IsNullOrEmpty(CommonTools.UserMapping.Options.UserMappingFile))
            {
                Log.LogError("UserMappingFile is not set");

                throw new ArgumentNullException("UserMappingFile must be set on the TfsUserMappingToolOptions in CommonEnrichersConfig.");
                       }
         
            List<IdentityMapData> usersToMap = new List<IdentityMapData>();
            if (Options.OnlyListUsersInWorkItems)
            {
                Log.LogInformation("OnlyListUsersInWorkItems is true, only users in work items will be listed");
                List<WorkItemData> sourceWorkItems = Source.WorkItems.GetWorkItems(Options.WIQLQuery);
                Log.LogInformation("Processed {0} work items from Source", sourceWorkItems.Count);

                usersToMap = CommonTools.UserMapping.GetUsersInSourceMappedToTargetForWorkItems(this, sourceWorkItems);
                Log.LogInformation("Found {usersToMap} total mapped", usersToMap.Count);
            }
            else
            {
                Log.LogInformation("OnlyListUsersInWorkItems is false, all users will be listed");
                usersToMap = CommonTools.UserMapping.GetUsersInSourceMappedToTarget(this);
                Log.LogInformation("Found {usersToMap} total mapped", usersToMap.Count);
            }

            usersToMap = usersToMap.Where(x => x.Source.FriendlyName != x.target?.FriendlyName).ToList();
            Log.LogInformation("Filtered to {usersToMap} total viable mappings", usersToMap.Count);
            Dictionary<string, string> usermappings = usersToMap.ToDictionary(x => x.Source.FriendlyName, x => x.target?.FriendlyName);
            System.IO.File.WriteAllText(CommonTools.UserMapping.Options.UserMappingFile, Newtonsoft.Json.JsonConvert.SerializeObject(usermappings, Formatting.Indented));
            Log.LogInformation("Writen to: {LocalExportJsonFile}", CommonTools.UserMapping.Options.UserMappingFile);
            //////////////////////////////////////////////////
            stopwatch.Stop();
            Log.LogInformation("DONE in {Elapsed} seconds");
        }
    }
}