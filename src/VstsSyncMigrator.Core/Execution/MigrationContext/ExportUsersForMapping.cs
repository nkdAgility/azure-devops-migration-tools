using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MigrationTools;
using MigrationTools._EngineV1.Configuration;
using MigrationTools._EngineV1.Configuration.Processing;
using MigrationTools.DataContracts;
using MigrationTools.DataContracts.Process;
using MigrationTools.Enrichers;
using VstsSyncMigrator._EngineV1.Processors;

namespace VstsSyncMigrator.Core.Execution.MigrationContext
{
    public class ExportUsersForMapping : StaticProcessorBase
    {
        private ExportUsersForMappingConfig _config;
        private TfsUserMappingEnricher _TfsUserMappingEnricher;

        public ExportUsersForMapping(IServiceProvider services, IMigrationEngine me, ITelemetryLogger telemetry, ILogger<ExportUsersForMapping> logger) : base(services, me, telemetry, logger)
        {
            Logger = logger;
        }

        public override string Name
        {
            get
            {
                return "ExportUsersForMapping";
            }
        }

        public ILogger<ExportUsersForMapping> Logger { get; }

        public override void Configure(IProcessorConfig config)
        {
            _config = (ExportUsersForMappingConfig)config;
            _TfsUserMappingEnricher = Services.GetRequiredService<TfsUserMappingEnricher>();
        }

        protected override void InternalExecute()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();
            //////////////////////////////////////////////////
            List<WorkItemData> sourceWorkItems = Engine.Source.WorkItems.GetWorkItems(_config.WIQLQuery);
            Log.LogInformation("Processing {0} work items from Source", sourceWorkItems.Count);
            /////////////////////////////////////////////////

           Dictionary<string,string> usersToMap = _TfsUserMappingEnricher.findUsersToMap(sourceWorkItems, _config.IdentityFieldsToCheck);

            System.IO.File.WriteAllText(_config.LocalExportJsonFile, Newtonsoft.Json.JsonConvert.SerializeObject(usersToMap));

            //////////////////////////////////////////////////
            stopwatch.Stop();
            Log.LogInformation("DONE in {Elapsed} seconds");
        }
    }
}