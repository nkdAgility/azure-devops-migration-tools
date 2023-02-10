using System;
using Microsoft.Extensions.Logging;
using MigrationTools;
using MigrationTools._EngineV1.Configuration;
using MigrationTools._EngineV1.Configuration.Processing;
using MigrationTools._EngineV1.Processors;

namespace VstsSyncMigrator.Engine
{
    /// <summary>
    /// Migrates Teams and Team Settings  
    /// </summary>
    /// <status>preview</status>
    /// <processingtarget>Teams</processingtarget>
    public class TeamMigrationContext : MigrationProcessorBase
    {
        private TeamMigrationConfig _config;

        public TeamMigrationContext(IMigrationEngine engine, IServiceProvider services, ITelemetryLogger telemetry, ILogger<TeamMigrationContext> logger) : base(engine, services, telemetry, logger)
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
            Log.LogCritical("TeamMigrationContext has been migrated to TfsTeamSettingsProcessor: https://nkdagility.github.io/azure-devops-migration-tools/Reference/Processors/TfsTeamSettingsProcessor.html");
        }
    }
}