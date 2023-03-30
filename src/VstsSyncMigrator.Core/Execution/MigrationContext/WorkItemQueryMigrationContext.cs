using System;
using Microsoft.Extensions.Logging;
using MigrationTools;
using MigrationTools._EngineV1.Configuration;
using MigrationTools._EngineV1.Configuration.Processing;
using MigrationTools._EngineV1.Processors;

namespace VstsSyncMigrator.Engine
{
    /// <summary>
    /// This processor can migrate queries for work items. Only shared queries are included. Personal queries can't migrate with this tool.
    /// </summary>
    /// <status>preview</status>
    /// <processingtarget>Shared Queries</processingtarget>
    [Obsolete("WorkItemQueryMigrationContext has been migrated to TfsSharedQueryProcessor: https://nkdagility.com/docs/azure-devops-migration-tools/Reference/Processors/TfsSharedQueryProcessor.html")]
    public class WorkItemQueryMigrationContext : MigrationProcessorBase
    {
        /// <summary>
        /// The processor configuration
        /// </summary>
        private WorkItemQueryMigrationConfig config;

        public WorkItemQueryMigrationContext(IMigrationEngine engine, IServiceProvider services, ITelemetryLogger telemetry, ILogger<WorkItemQueryMigrationContext> logger) : base(engine, services, telemetry, logger)
        {
        }

        public override string Name
        {
            get
            {
                return "WorkItemQueryMigrationProcessorContext";
            }
        }

        public override void Configure(IProcessorConfig config)
        {
            this.config = (WorkItemQueryMigrationConfig)config;
        }

        protected override void InternalExecute()
        {
            Log.LogCritical("*************MIGRATION ALERT!!!!!!!!!");
            Log.LogCritical("WorkItemQueryMigrationContext has been migrated to TfsSharedQueryProcessor: https://nkdagility.com/docs/azure-devops-migration-tools/Reference/Processors/TfsSharedQueryProcessor.html");
        }
    }
}