using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MigrationTools;
using MigrationTools._EngineV1.Configuration;
using MigrationTools._EngineV1.Configuration.Processing;
using MigrationTools._EngineV1.Processors;
using MigrationTools.DataContracts;
using MigrationTools.DataContracts.Process;
using MigrationTools.EndpointEnrichers;
using MigrationTools.Enrichers;
using MigrationTools.ProcessorEnrichers;
using MigrationTools.ProcessorEnrichers.WorkItemProcessorEnrichers;
using Newtonsoft.Json;
using VstsSyncMigrator._EngineV1.Processors;

namespace VstsSyncMigrator.Core.Execution.MigrationContext
{
    /// <summary>
    /// ExportUsersForMappingContext is a tool used to create a starter mapping file for users between the source and target systems.
    /// Use `ExportUsersForMappingConfig` to configure.
    /// </summary>
    /// <status>ready</status>
    /// <processingtarget>Work Items</processingtarget>
    public class ExportUsersForMappingContext : TfsMigrationProcessorBase
    {
        private ExportUsersForMappingConfig _config;
        private TfsUserMappingEnricher _TfsUserMappingEnricher;



        public override string Name
        {
            get
            {
                return "ExportUsersForMappingContext";
            }
        }

        public ILogger<ExportUsersForMappingContext> Logger { get; }

        private EngineConfiguration _engineConfig;

        public ExportUsersForMappingContext(IOptions<ExportUsersForMappingConfig> options, IOptions<EngineConfiguration> engineConfig, IMigrationEngine engine, TfsStaticEnrichers tfsStaticEnrichers, StaticEnrichers staticEnrichers, IServiceProvider services, ITelemetryLogger telemetry, ILogger<ExportUsersForMappingContext> logger) : base(engine, tfsStaticEnrichers, staticEnrichers, services, telemetry, logger)
        {
            Logger = logger;
            _engineConfig =  engineConfig.Value;
            _config = options.Value;
        }

        protected override void InternalExecute()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            if(string.IsNullOrEmpty(_TfsUserMappingEnricher.Options.UserMappingFile))
            {
                Log.LogError("UserMappingFile is not set");

                throw new ArgumentNullException("UserMappingFile must be set on the TfsUserMappingEnricherOptions in CommonEnrichersConfig.");
                       }
         
            List<IdentityMapData> usersToMap = new List<IdentityMapData>();
            if (_config.OnlyListUsersInWorkItems)
            {
                Log.LogInformation("OnlyListUsersInWorkItems is true, only users in work items will be listed");
                List<WorkItemData> sourceWorkItems = Engine.Source.WorkItems.GetWorkItems(_config.WIQLQuery);
                Log.LogInformation("Processed {0} work items from Source", sourceWorkItems.Count);

                usersToMap = _TfsUserMappingEnricher.GetUsersInSourceMappedToTargetForWorkItems(sourceWorkItems);
                Log.LogInformation("Found {usersToMap} total mapped", usersToMap.Count);
            }
            else
            {
                Log.LogInformation("OnlyListUsersInWorkItems is false, all users will be listed");
                usersToMap = _TfsUserMappingEnricher.GetUsersInSourceMappedToTarget();
                Log.LogInformation("Found {usersToMap} total mapped", usersToMap.Count);
            }

            usersToMap = usersToMap.Where(x => x.Source.FriendlyName != x.target?.FriendlyName).ToList();
            Log.LogInformation("Filtered to {usersToMap} total viable mappings", usersToMap.Count);
            Dictionary<string, string> usermappings = usersToMap.ToDictionary(x => x.Source.FriendlyName, x => x.target?.FriendlyName);
            System.IO.File.WriteAllText(_TfsUserMappingEnricher.Options.UserMappingFile, Newtonsoft.Json.JsonConvert.SerializeObject(usermappings, Formatting.Indented));
            Log.LogInformation("Writen to: {LocalExportJsonFile}", _TfsUserMappingEnricher.Options.UserMappingFile);
            //////////////////////////////////////////////////
            stopwatch.Stop();
            Log.LogInformation("DONE in {Elapsed} seconds");
        }
    }
}