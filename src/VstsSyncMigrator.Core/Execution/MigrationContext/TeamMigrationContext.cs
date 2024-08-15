using System;
using Microsoft.Extensions.Logging;
using MigrationTools;
using MigrationTools._EngineV1.Configuration;
using MigrationTools._EngineV1.Configuration.Processing;
using MigrationTools._EngineV1.Processors;
using MigrationTools.Enrichers;
using MigrationTools.ProcessorEnrichers;
using VstsSyncMigrator.Core.Execution;

namespace VstsSyncMigrator.Engine
{
    /// <summary>
    /// Migrates Teams and Team Settings: This should be run after `NodeStructuresMigrationConfig` and before all other processors.
    /// </summary>
    /// <status>preview</status>
    /// <processingtarget>Teams</processingtarget>
    public class TeamMigrationContext : TfsMigrationProcessorBase
    {
        private TeamMigrationConfig _config;

        public TeamMigrationContext(IMigrationEngine engine, TfsStaticEnrichers tfsStaticEnrichers, StaticEnrichers staticEnrichers, IServiceProvider services, ITelemetryLogger telemetry, ILogger<MigrationProcessorBase> logger) : base(engine, tfsStaticEnrichers, staticEnrichers, services, telemetry, logger)
        {
        }

        public override string Name
        {
            get
            {
                return "TeamMigrationContext";
            }
        }

        public override void Configure(IProcessorConfig config)
        {
            _config = (TeamMigrationConfig)config;
        }

        protected override void InternalExecute()
        {
            Log.LogCritical("*************MIGRATION ALERT!!!!!!!!!");
            Log.LogCritical("TeamMigrationContext has been migrated to TfsTeamSettingsProcessor: https://nkdagility.com/docs/azure-devops-migration-tools/Reference/Processors/TfsTeamSettingsProcessor.html");
        }
    }
}