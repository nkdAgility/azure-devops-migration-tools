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

using MigrationTools.DataContracts;
using MigrationTools.DataContracts.Process;
using MigrationTools.EndpointEnrichers;
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
    public class ExportUsersForMappingProcessor : TfsMigrationProcessorBase
    {
        private ExportUsersForMappingProcessorOptions _config;
        private TfsUserMappingTool _TfsUserMappingTool;

        public override string Name
        {
            get
            {
                return "ExportUsersForMappingProcessor";
            }
        }

        public ILogger<ExportUsersForMappingProcessor> Logger { get; }

        private EngineConfiguration _engineConfig;

        public ExportUsersForMappingProcessor(IOptions<ExportUsersForMappingProcessorOptions> options, IOptions<EngineConfiguration> engineConfig, IMigrationEngine engine, TfsStaticTools tfsStaticEnrichers, StaticTools staticEnrichers, IServiceProvider services, ITelemetryLogger telemetry, ILogger<ExportUsersForMappingProcessor> logger) : base(engine, tfsStaticEnrichers, staticEnrichers, services, telemetry, logger)
        {
            Logger = logger;
            _engineConfig =  engineConfig.Value;
            _config = options.Value;
        }

        protected override void InternalExecute()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            if(string.IsNullOrEmpty(_TfsUserMappingTool.Options.UserMappingFile))
            {
                Log.LogError("UserMappingFile is not set");

                throw new ArgumentNullException("UserMappingFile must be set on the TfsUserMappingToolOptions in CommonEnrichersConfig.");
                       }
         
            List<IdentityMapData> usersToMap = new List<IdentityMapData>();
            if (_config.OnlyListUsersInWorkItems)
            {
                Log.LogInformation("OnlyListUsersInWorkItems is true, only users in work items will be listed");
                List<WorkItemData> sourceWorkItems = Engine.Source.WorkItems.GetWorkItems(_config.WIQLQuery);
                Log.LogInformation("Processed {0} work items from Source", sourceWorkItems.Count);

                usersToMap = _TfsUserMappingTool.GetUsersInSourceMappedToTargetForWorkItems(sourceWorkItems);
                Log.LogInformation("Found {usersToMap} total mapped", usersToMap.Count);
            }
            else
            {
                Log.LogInformation("OnlyListUsersInWorkItems is false, all users will be listed");
                usersToMap = _TfsUserMappingTool.GetUsersInSourceMappedToTarget();
                Log.LogInformation("Found {usersToMap} total mapped", usersToMap.Count);
            }

            usersToMap = usersToMap.Where(x => x.Source.FriendlyName != x.target?.FriendlyName).ToList();
            Log.LogInformation("Filtered to {usersToMap} total viable mappings", usersToMap.Count);
            Dictionary<string, string> usermappings = usersToMap.ToDictionary(x => x.Source.FriendlyName, x => x.target?.FriendlyName);
            System.IO.File.WriteAllText(_TfsUserMappingTool.Options.UserMappingFile, Newtonsoft.Json.JsonConvert.SerializeObject(usermappings, Formatting.Indented));
            Log.LogInformation("Writen to: {LocalExportJsonFile}", _TfsUserMappingTool.Options.UserMappingFile);
            //////////////////////////////////////////////////
            stopwatch.Stop();
            Log.LogInformation("DONE in {Elapsed} seconds");
        }
    }
}